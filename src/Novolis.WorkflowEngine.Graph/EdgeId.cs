namespace Novolis.WorkflowEngine.Graph;

/// <summary>
/// Stable identity of a graph edge.
/// </summary>
public readonly record struct EdgeId(Guid Value) : IComparable<EdgeId>
{
    /// <summary>
    /// An empty edge identity.
    /// </summary>
    public static EdgeId Empty => new(Guid.Empty);

    /// <summary>
    /// Creates a new edge identity.
    /// </summary>
    public static EdgeId New() => new(Guid.NewGuid());

    /// <inheritdoc />
    public int CompareTo(EdgeId other) => Value.CompareTo(other.Value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString("N");
}
