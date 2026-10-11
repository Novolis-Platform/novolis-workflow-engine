namespace Novolis.WorkflowEngine.Flow;

/// <summary>
/// Describes one typed port on a flow node.
/// </summary>
public sealed record PortDescriptor
{
    /// <summary>
    /// Creates a port descriptor.
    /// </summary>
    public PortDescriptor(
        PortId id,
        PortDirection direction,
        FlowType type,
        PortCardinality? cardinality = null)
    {
        if (string.IsNullOrWhiteSpace(id.Value))
        {
            throw new ArgumentException("A port must have a non-empty identity.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(type);
        Id = id;
        Direction = direction;
        Type = type;
        Cardinality = cardinality ?? (
            direction == PortDirection.Output
                ? PortCardinality.Many
                : PortCardinality.Single);
    }

    /// <summary>The stable port identity.</summary>
    public PortId Id { get; }

    /// <summary>The port direction.</summary>
    public PortDirection Direction { get; }

    /// <summary>The semantic value type.</summary>
    public FlowType Type { get; }

    /// <summary>The connection cardinality.</summary>
    public PortCardinality Cardinality { get; }

    /// <summary>Gets a value indicating whether this is an input port.</summary>
    public bool IsInput => Direction == PortDirection.Input;

    /// <summary>Gets a value indicating whether this is an output port.</summary>
    public bool IsOutput => Direction == PortDirection.Output;

    /// <summary>
    /// Determines whether another connection can be added at a given count.
    /// </summary>
    public bool AllowsConnection(int existingConnectionCount)
    {
        if (existingConnectionCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(existingConnectionCount));
        }

        return Cardinality == PortCardinality.Many || existingConnectionCount == 0;
    }
}
