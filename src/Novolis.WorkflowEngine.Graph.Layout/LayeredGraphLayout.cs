using System.Collections.Immutable;
using Novolis.WorkflowEngine.Graph;

namespace Novolis.WorkflowEngine.Graph.Layout;

/// <summary>
/// Produces a stable left-to-right layered layout.
/// </summary>
public sealed class LayeredGraphLayout : IGraphLayoutEngine
{
    /// <inheritdoc />
    public GraphLayoutResult Layout<T>(
        Graph<T> graph,
        Func<GraphNode<T>, string>? labelSelector = null,
        GraphViewState? viewState = null,
        GraphLayoutOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(graph);
        options ??= new GraphLayoutOptions();
        options.Validate();
        viewState ??= new GraphViewState();

        var layers = CalculateLayers(graph);
        var positions = new Dictionary<NodeId, GraphRectangle>();
        var pinned = new HashSet<NodeId>();
        var nodesByLayer = graph.Nodes
            .GroupBy(node => layers[node.Id])
            .OrderBy(group => group.Key);

        foreach (var layer in nodesByLayer)
        {
            var index = 0;
            foreach (var node in layer.OrderBy(candidate => candidate.Id))
            {
                var key = node.Id.ToString();
                var pinnedPosition = default(GraphPoint);
                var isPinned = options.RespectPinnedPositions &&
                    viewState.PinnedPositions.TryGetValue(key, out pinnedPosition);
                var point = isPinned
                    ? pinnedPosition
                    : new GraphPoint(
                        options.CanvasPadding +
                        layer.Key * (options.NodeWidth + options.LayerSpacing),
                        options.CanvasPadding +
                        index * (options.NodeHeight + options.NodeSpacing));

                positions[node.Id] = new GraphRectangle(
                    point.X,
                    point.Y,
                    options.NodeWidth,
                    options.NodeHeight);
                if (isPinned)
                {
                    pinned.Add(node.Id);
                }

                index++;
            }
        }

        var layoutNodes = graph.Nodes
            .OrderBy(node => node.Id)
            .Select(node => new GraphLayoutNode(
                node.Id.ToString(),
                labelSelector?.Invoke(node) ?? FormatLabel(node),
                positions[node.Id],
                layers[node.Id],
                pinned.Contains(node.Id)))
            .ToImmutableArray();

        var layoutEdges = graph.Edges
            .OrderBy(edge => edge.Id)
            .Where(edge => positions.ContainsKey(edge.From) &&
                           positions.ContainsKey(edge.To))
            .Select(edge => new GraphLayoutEdge(
                edge.From.ToString(),
                edge.To.ToString(),
                Route(
                    positions[edge.From],
                    positions[edge.To],
                    layers[edge.From],
                    layers[edge.To],
                    options)))
            .ToImmutableArray();

        return new GraphLayoutResult
        {
            Nodes = layoutNodes,
            Edges = layoutEdges,
            Bounds = CalculateBounds(layoutNodes),
        };
    }

    private static string FormatLabel<T>(GraphNode<T> node)
    {
        var value = node.Value?.ToString();
        return string.IsNullOrWhiteSpace(value)
            ? node.Id.ToString()
            : value;
    }

    private static Dictionary<NodeId, int> CalculateLayers<T>(Graph<T> graph)
    {
        var nodes = graph.Nodes.OrderBy(node => node.Id).ToArray();
        var layers = nodes.ToDictionary(node => node.Id, _ => 0);
        var indegree = nodes.ToDictionary(node => node.Id, _ => 0);
        var outgoing = nodes.ToDictionary(
            node => node.Id,
            _ => new SortedSet<NodeId>());

        foreach (var edge in graph.Edges.OrderBy(edge => edge.Id))
        {
            if (!outgoing.TryGetValue(edge.From, out var destinations) ||
                !indegree.ContainsKey(edge.To) ||
                !destinations.Add(edge.To))
            {
                continue;
            }

            indegree[edge.To]++;
        }

        var pending = new PriorityQueue<NodeId, NodeId>();
        foreach (var node in nodes.Where(node => indegree[node.Id] == 0))
        {
            pending.Enqueue(node.Id, node.Id);
        }

        var visited = new HashSet<NodeId>();
        while (pending.TryDequeue(out var current, out _))
        {
            visited.Add(current);
            foreach (var destination in outgoing[current])
            {
                layers[destination] = Math.Max(
                    layers[destination],
                    layers[current] + 1);
                indegree[destination]--;
                if (indegree[destination] == 0)
                {
                    pending.Enqueue(destination, destination);
                }
            }
        }

        var nextLayer = layers.Values.DefaultIfEmpty(0).Max() + 1;
        foreach (var node in nodes.Where(node => !visited.Contains(node.Id)))
        {
            layers[node.Id] = nextLayer++;
        }

        return layers;
    }

    private static IReadOnlyList<GraphPoint> Route(
        GraphRectangle source,
        GraphRectangle target,
        int sourceLayer,
        int targetLayer,
        GraphLayoutOptions options)
    {
        var start = new GraphPoint(source.Right, source.CenterY);
        var end = new GraphPoint(target.X, target.CenterY);
        if (targetLayer > sourceLayer && target.X >= source.Right)
        {
            return [start, end];
        }

        var bendX = Math.Max(source.Right, target.Right) +
            options.LayerSpacing / 2d;
        return
        [
            start,
            new GraphPoint(bendX, start.Y),
            new GraphPoint(bendX, end.Y),
            end,
        ];
    }

    private static GraphRectangle CalculateBounds(
        IReadOnlyList<GraphLayoutNode> nodes)
    {
        if (nodes.Count == 0)
        {
            return new GraphRectangle(0d, 0d, 0d, 0d);
        }

        var minX = nodes.Min(node => node.Bounds.X);
        var minY = nodes.Min(node => node.Bounds.Y);
        var maxX = nodes.Max(node => node.Bounds.Right);
        var maxY = nodes.Max(node => node.Bounds.Bottom);
        return new GraphRectangle(
            minX,
            minY,
            maxX - minX,
            maxY - minY);
    }
}
