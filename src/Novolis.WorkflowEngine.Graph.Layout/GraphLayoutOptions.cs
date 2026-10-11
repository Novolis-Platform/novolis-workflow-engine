namespace Novolis.WorkflowEngine.Graph.Layout;

/// <summary>
/// Deterministic layout parameters.
/// </summary>
public sealed record GraphLayoutOptions
{
    /// <summary>Horizontal space between layers.</summary>
    public double LayerSpacing { get; init; } = 180d;

    /// <summary>Vertical space between nodes in a layer.</summary>
    public double NodeSpacing { get; init; } = 48d;

    /// <summary>Default node width.</summary>
    public double NodeWidth { get; init; } = 160d;

    /// <summary>Default node height.</summary>
    public double NodeHeight { get; init; } = 80d;

    /// <summary>Left and top canvas padding.</summary>
    public double CanvasPadding { get; init; } = 32d;

    /// <summary>Whether editor pins should override calculated positions.</summary>
    public bool RespectPinnedPositions { get; init; } = true;

    internal void Validate()
    {
        if (LayerSpacing < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(LayerSpacing));
        }

        if (NodeSpacing < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(NodeSpacing));
        }

        if (NodeWidth <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(NodeWidth));
        }

        if (NodeHeight <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(NodeHeight));
        }

        if (CanvasPadding < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(CanvasPadding));
        }
    }
}
