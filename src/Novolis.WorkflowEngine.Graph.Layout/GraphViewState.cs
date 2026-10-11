using System.Collections.Immutable;

namespace Novolis.WorkflowEngine.Graph.Layout;

/// <summary>
/// Editor view state kept separate from graph semantics.
/// </summary>
public sealed record GraphViewState
{
    /// <summary>The graph camera.</summary>
    public GraphViewport Viewport { get; init; } = GraphViewport.Default;

    /// <summary>Nodes collapsed in the editor.</summary>
    public ImmutableHashSet<string> CollapsedNodeIds { get; init; } =
        ImmutableHashSet<string>.Empty;

    /// <summary>Optional user-pinned node positions.</summary>
    public ImmutableDictionary<string, GraphPoint> PinnedPositions { get; init; } =
        ImmutableDictionary<string, GraphPoint>.Empty;
}
