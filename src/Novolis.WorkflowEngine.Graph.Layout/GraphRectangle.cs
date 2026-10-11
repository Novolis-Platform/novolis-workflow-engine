namespace Novolis.WorkflowEngine.Graph.Layout;

/// <summary>
/// A node or graph bounds rectangle.
/// </summary>
public readonly record struct GraphRectangle(
    double X,
    double Y,
    double Width,
    double Height)
{
    /// <summary>The right edge.</summary>
    public double Right => X + Width;

    /// <summary>The bottom edge.</summary>
    public double Bottom => Y + Height;

    /// <summary>The horizontal center.</summary>
    public double CenterX => X + Width / 2d;

    /// <summary>The vertical center.</summary>
    public double CenterY => Y + Height / 2d;

    /// <summary>The rectangle center.</summary>
    public GraphPoint Center => new(CenterX, CenterY);
}
