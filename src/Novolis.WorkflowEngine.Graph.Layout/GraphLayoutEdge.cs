namespace Novolis.WorkflowEngine.Graph.Layout;

/// <summary>
/// A routed graph edge.
/// </summary>
public sealed record GraphLayoutEdge(
    string SourceId,
    string TargetId,
    IReadOnlyList<GraphPoint> Route);
