using System.Collections.Frozen;

namespace Novolis.WorkflowEngine.Graph;

/// <summary>
/// Performs deterministic structural graph analysis.
/// </summary>
public sealed class GraphAnalyzer : IGraphAnalyzer
{
    /// <summary>
    /// Shared stateless analyzer.
    /// </summary>
    public static GraphAnalyzer Default { get; } = new();

    /// <inheritdoc />
    public GraphAnalysis Analyze<T>(
        Graph<T> graph,
        GraphAnalysisOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(graph);
        options ??= GraphAnalysisOptions.Default;

        var diagnostics = new List<GraphDiagnostic>();
        var nodeGroups = graph.Nodes
            .GroupBy(node => node.Id)
            .OrderBy(group => group.Key)
            .ToArray();
        var nodeIds = nodeGroups.Select(group => group.Key).ToArray();
        var nodeSet = nodeIds.ToFrozenSet();

        foreach (var group in nodeGroups.Where(group => group.Count() > 1))
        {
            diagnostics.Add(new GraphDiagnostic(
                GraphDiagnosticCode.DuplicateNodeId,
                GraphDiagnosticSeverity.Error,
                $"Node identity '{group.Key}' occurs {group.Count()} times.",
                NodeId: group.Key));
        }

        var adjacency = nodeIds.ToDictionary(
            nodeId => nodeId,
            _ => new SortedSet<NodeId>());
        var edgeIdGroups = graph.Edges
            .GroupBy(edge => edge.Id)
            .OrderBy(group => group.Key)
            .ToArray();
        var endpointPairs = new HashSet<(NodeId From, NodeId To)>();

        foreach (var group in edgeIdGroups.Where(group => group.Count() > 1))
        {
            diagnostics.Add(new GraphDiagnostic(
                GraphDiagnosticCode.DuplicateEdgeId,
                GraphDiagnosticSeverity.Error,
                $"Edge identity '{group.Key}' occurs {group.Count()} times.",
                EdgeId: group.Key));
        }

        foreach (var edge in graph.Edges.OrderBy(candidate => candidate.Id))
        {
            var hasSource = nodeSet.Contains(edge.From);
            var hasDestination = nodeSet.Contains(edge.To);

            if (!hasSource)
            {
                diagnostics.Add(new GraphDiagnostic(
                    GraphDiagnosticCode.MissingSourceNode,
                    GraphDiagnosticSeverity.Error,
                    $"Edge '{edge.Id}' refers to missing source node '{edge.From}'.",
                    NodeId: edge.From,
                    EdgeId: edge.Id));
            }

            if (!hasDestination)
            {
                diagnostics.Add(new GraphDiagnostic(
                    GraphDiagnosticCode.MissingDestinationNode,
                    GraphDiagnosticSeverity.Error,
                    $"Edge '{edge.Id}' refers to missing destination node '{edge.To}'.",
                    NodeId: edge.To,
                    EdgeId: edge.Id));
            }

            if (hasSource && hasDestination)
            {
                if (!endpointPairs.Add((edge.From, edge.To)))
                {
                    diagnostics.Add(new GraphDiagnostic(
                        GraphDiagnosticCode.DuplicateEdgeEndpoints,
                        options.RejectDuplicateEdgeEndpoints
                            ? GraphDiagnosticSeverity.Error
                            : GraphDiagnosticSeverity.Warning,
                        $"Multiple edges connect '{edge.From}' to '{edge.To}'.",
                        EdgeId: edge.Id));
                }

                adjacency[edge.From].Add(edge.To);
            }

            if (edge.From == edge.To)
            {
                diagnostics.Add(new GraphDiagnostic(
                    GraphDiagnosticCode.SelfLoop,
                    options.RejectSelfLoops
                        ? GraphDiagnosticSeverity.Error
                        : GraphDiagnosticSeverity.Warning,
                    $"Edge '{edge.Id}' is a self-loop on node '{edge.From}'.",
                    NodeId: edge.From,
                    EdgeId: edge.Id));
            }
        }

        var components = FindStronglyConnectedComponents(nodeIds, adjacency);
        var cyclicComponents = components
            .Where(component =>
                component.Count > 1 ||
                (component.Count == 1 && adjacency[component[0]].Contains(component[0])))
            .ToArray();

        foreach (var component in cyclicComponents)
        {
            diagnostics.Add(new GraphDiagnostic(
                GraphDiagnosticCode.Cycle,
                options.RejectCycles
                    ? GraphDiagnosticSeverity.Error
                    : GraphDiagnosticSeverity.Warning,
                $"Graph contains a cycle involving {string.Join(", ", component)}.",
                NodeId: component[0]));
        }

        if (options.RequireReachabilityFromRoots)
        {
            var roots = options.Roots?.Where(nodeSet.Contains).ToArray()
                ?? nodeIds.Where(nodeId =>
                    !graph.Edges.Any(edge => edge.To == nodeId && nodeSet.Contains(edge.From)))
                    .ToArray();
            var reachable = Reachable(nodeSet, adjacency, roots);
            foreach (var inaccessible in nodeIds.Where(nodeId => !reachable.Contains(nodeId)))
            {
                diagnostics.Add(new GraphDiagnostic(
                    GraphDiagnosticCode.InaccessibleNode,
                    GraphDiagnosticSeverity.Error,
                    $"Node '{inaccessible}' is not reachable from the configured roots.",
                    NodeId: inaccessible));
            }
        }

        var isAcyclic = cyclicComponents.Length == 0;
        var hasIdentityOrEndpointErrors = diagnostics.Any(diagnostic =>
            diagnostic.Severity == GraphDiagnosticSeverity.Error &&
            diagnostic.Code is
                GraphDiagnosticCode.DuplicateNodeId or
                GraphDiagnosticCode.DuplicateEdgeId or
                GraphDiagnosticCode.MissingSourceNode or
                GraphDiagnosticCode.MissingDestinationNode);
        var topologicalOrder = isAcyclic && !hasIdentityOrEndpointErrors
            ? CreateTopologicalOrder(nodeIds, adjacency)
            : [];

        diagnostics.Sort(CompareDiagnostics);
        return new GraphAnalysis(
            diagnostics,
            components,
            topologicalOrder,
            isAcyclic);
    }

    /// <summary>
    /// Computes the changed nodes and every node reachable downstream from them.
    /// </summary>
    /// <typeparam name="T">The node value type.</typeparam>
    /// <param name="graph">The graph to traverse.</param>
    /// <param name="changedNodeIds">The changed node identities.</param>
    /// <returns>The affected node identities in stable order.</returns>
    public IReadOnlySet<NodeId> AffectedDownstream<T>(
        Graph<T> graph,
        IEnumerable<NodeId> changedNodeIds)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(changedNodeIds);

        var knownNodes = graph.Nodes.Select(node => node.Id).ToFrozenSet();
        var affected = new HashSet<NodeId>(
            changedNodeIds.Where(knownNodes.Contains));
        var pending = new Queue<NodeId>(affected.OrderBy(nodeId => nodeId));

        while (pending.Count > 0)
        {
            var current = pending.Dequeue();
            foreach (var downstream in graph.Downstream(current))
            {
                if (affected.Add(downstream))
                {
                    pending.Enqueue(downstream);
                }
            }
        }

        return affected.ToFrozenSet();
    }

    /// <summary>
    /// Creates a deterministic topological order for an acyclic graph.
    /// </summary>
    /// <typeparam name="T">The node value type.</typeparam>
    /// <param name="graph">The graph to order.</param>
    /// <returns>The topological order.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the graph is invalid or cyclic.
    /// </exception>
    public IReadOnlyList<NodeId> TopologicalSort<T>(Graph<T> graph)
    {
        var analysis = Analyze(graph);
        if (!analysis.IsValid || !analysis.IsAcyclic)
        {
            throw new InvalidOperationException(
                "The graph cannot be topologically ordered under the default policy.");
        }

        return analysis.TopologicalOrder;
    }

    private static IReadOnlyList<IReadOnlyList<NodeId>> FindStronglyConnectedComponents(
        IReadOnlyList<NodeId> nodeIds,
        IReadOnlyDictionary<NodeId, SortedSet<NodeId>> adjacency)
    {
        var index = 0;
        var indices = new Dictionary<NodeId, int>();
        var lowLinks = new Dictionary<NodeId, int>();
        var stack = new Stack<NodeId>();
        var onStack = new HashSet<NodeId>();
        var components = new List<IReadOnlyList<NodeId>>();

        void Visit(NodeId nodeId)
        {
            indices[nodeId] = index;
            lowLinks[nodeId] = index;
            index++;
            stack.Push(nodeId);
            onStack.Add(nodeId);

            foreach (var next in adjacency[nodeId])
            {
                if (!indices.ContainsKey(next))
                {
                    Visit(next);
                    lowLinks[nodeId] = Math.Min(lowLinks[nodeId], lowLinks[next]);
                }
                else if (onStack.Contains(next))
                {
                    lowLinks[nodeId] = Math.Min(lowLinks[nodeId], indices[next]);
                }
            }

            if (lowLinks[nodeId] != indices[nodeId])
            {
                return;
            }

            var component = new List<NodeId>();
            NodeId member;
            do
            {
                member = stack.Pop();
                onStack.Remove(member);
                component.Add(member);
            }
            while (member != nodeId);

            component.Sort();
            components.Add(component);
        }

        foreach (var nodeId in nodeIds.OrderBy(candidate => candidate))
        {
            if (!indices.ContainsKey(nodeId))
            {
                Visit(nodeId);
            }
        }

        components.Sort((left, right) => left[0].CompareTo(right[0]));
        return components;
    }

    private static IReadOnlySet<NodeId> Reachable(
        IReadOnlySet<NodeId> nodeSet,
        IReadOnlyDictionary<NodeId, SortedSet<NodeId>> adjacency,
        IEnumerable<NodeId> roots)
    {
        var reachable = new HashSet<NodeId>();
        var pending = new Queue<NodeId>();
        foreach (var root in roots.OrderBy(nodeId => nodeId))
        {
            if (nodeSet.Contains(root) && reachable.Add(root))
            {
                pending.Enqueue(root);
            }
        }

        while (pending.Count > 0)
        {
            var current = pending.Dequeue();
            foreach (var next in adjacency[current])
            {
                if (reachable.Add(next))
                {
                    pending.Enqueue(next);
                }
            }
        }

        return reachable;
    }

    private static IReadOnlyList<NodeId> CreateTopologicalOrder(
        IReadOnlyList<NodeId> nodeIds,
        IReadOnlyDictionary<NodeId, SortedSet<NodeId>> adjacency)
    {
        var incoming = nodeIds.ToDictionary(nodeId => nodeId, _ => 0);
        foreach (var destinations in adjacency.Values)
        {
            foreach (var destination in destinations)
            {
                incoming[destination]++;
            }
        }

        var ready = new SortedSet<NodeId>(
            incoming.Where(pair => pair.Value == 0).Select(pair => pair.Key));
        var order = new List<NodeId>(nodeIds.Count);

        while (ready.Count > 0)
        {
            var current = ready.Min;
            ready.Remove(current);
            order.Add(current);

            foreach (var next in adjacency[current])
            {
                incoming[next]--;
                if (incoming[next] == 0)
                {
                    ready.Add(next);
                }
            }
        }

        return order;
    }

    private static int CompareDiagnostics(
        GraphDiagnostic left,
        GraphDiagnostic right)
    {
        var codeComparison = left.Code.CompareTo(right.Code);
        if (codeComparison != 0)
        {
            return codeComparison;
        }

        var nodeComparison = Nullable.Compare(left.NodeId, right.NodeId);
        if (nodeComparison != 0)
        {
            return nodeComparison;
        }

        return Nullable.Compare(left.EdgeId, right.EdgeId);
    }
}
