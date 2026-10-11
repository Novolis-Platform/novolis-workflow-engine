using Novolis.WorkflowEngine.Graph;

namespace Novolis.WorkflowEngine.Graph.Layout;

/// <summary>
/// Produces a deterministic editor projection from a semantic graph.
/// </summary>
public interface IGraphLayoutEngine
{
    /// <summary>Lays out a graph using the supplied editor state.</summary>
    GraphLayoutResult Layout<T>(
        Graph<T> graph,
        Func<GraphNode<T>, string>? labelSelector = null,
        GraphViewState? viewState = null,
        GraphLayoutOptions? options = null);
}
