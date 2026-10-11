namespace Novolis.WorkflowEngine.Graph.Export.Svg;

/// <summary>
/// Styling and sizing options for SVG graph output.
/// </summary>
public sealed record SvgGraphExportOptions
{
    /// <summary>Outer canvas padding.</summary>
    public double Padding { get; init; } = 24d;

    /// <summary>Minimum canvas width.</summary>
    public double MinimumWidth { get; init; } = 160d;

    /// <summary>Minimum canvas height.</summary>
    public double MinimumHeight { get; init; } = 100d;

    /// <summary>Node fill color.</summary>
    public string NodeFill { get; init; } = "#f5f7fb";

    /// <summary>Node border color.</summary>
    public string NodeStroke { get; init; } = "#3d4a5c";

    /// <summary>Node text color.</summary>
    public string TextColor { get; init; } = "#17202a";

    /// <summary>Edge color.</summary>
    public string EdgeColor { get; init; } = "#5c6b7a";

    /// <summary>Node corner radius.</summary>
    public double CornerRadius { get; init; } = 8d;

    /// <summary>Node border width.</summary>
    public double NodeStrokeWidth { get; init; } = 1.5d;

    /// <summary>Edge width.</summary>
    public double EdgeWidth { get; init; } = 1.5d;

    internal void Validate()
    {
        if (Padding < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(Padding));
        }

        if (MinimumWidth <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(MinimumWidth));
        }

        if (MinimumHeight <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(MinimumHeight));
        }

        if (CornerRadius < 0d ||
            NodeStrokeWidth <= 0d ||
            EdgeWidth <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(NodeStrokeWidth));
        }
    }
}
