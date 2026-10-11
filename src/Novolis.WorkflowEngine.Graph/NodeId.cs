namespace Novolis.WorkflowEngine.Graph;

/// <summary>
/// Stable identity of a graph node.
/// </summary>
public readonly record struct NodeId(Guid Value) : IComparable<NodeId>
{
    /// <summary>
    /// An empty node identity.
    /// </summary>
    public static NodeId Empty => new(Guid.Empty);

    /// <summary>
    /// Creates a new node identity.
    /// </summary>
    public static NodeId New() => new(Guid.NewGuid());

    /// <inheritdoc />
    public int CompareTo(NodeId other) => Value.CompareTo(other.Value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString("N");
}
