namespace Novolis.WorkflowEngine.Graph.Layout;

/// <summary>
/// A positioned graph node.
/// </summary>
public sealed record GraphLayoutNode(
    string Id,
    string Label,
    GraphRectangle Bounds,
    int Layer,
    bool IsPinned = false);
