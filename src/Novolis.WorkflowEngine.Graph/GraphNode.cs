namespace Novolis.WorkflowEngine.Graph;

/// <summary>
/// A graph node with stable identity and caller-owned value.
/// </summary>
/// <typeparam name="T">The node value type.</typeparam>
public sealed record GraphNode<T>
{
    /// <summary>
    /// Creates a graph node.
    /// </summary>
    /// <param name="id">The stable node identity.</param>
    /// <param name="value">The node value.</param>
    public GraphNode(NodeId id, T value)
    {
        if (id == NodeId.Empty)
        {
            throw new ArgumentException("A graph node must have a non-empty identity.", nameof(id));
        }

        Id = id;
        Value = value;
    }

    /// <summary>The stable node identity.</summary>
    public NodeId Id { get; }

    /// <summary>The caller-owned node value.</summary>
    public T Value { get; }
}
