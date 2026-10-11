namespace Novolis.WorkflowEngine.Graph.Layout;

/// <summary>
/// The camera state used to display a graph projection.
/// </summary>
public readonly record struct GraphViewport(
    double X,
    double Y,
    double Zoom)
{
    /// <summary>Creates the default viewport.</summary>
    public static GraphViewport Default => new(0d, 0d, 1d);
}
