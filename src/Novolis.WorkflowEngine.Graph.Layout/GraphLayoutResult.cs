using System.Collections.Immutable;

namespace Novolis.WorkflowEngine.Graph.Layout;

/// <summary>
/// The complete deterministic graph projection.
/// </summary>
public sealed record GraphLayoutResult
{
    /// <summary>Positioned nodes, in deterministic order.</summary>
    public required ImmutableArray<GraphLayoutNode> Nodes { get; init; }

    /// <summary>Routed edges, in deterministic order.</summary>
    public required ImmutableArray<GraphLayoutEdge> Edges { get; init; }

    /// <summary>The canvas bounds containing all nodes.</summary>
    public required GraphRectangle Bounds { get; init; }

    /// <summary>Finds a node by semantic identifier.</summary>
    public GraphLayoutNode? FindNode(string id)
    {
        return Nodes.FirstOrDefault(node => string.Equals(node.Id, id, StringComparison.Ordinal));
    }
}
