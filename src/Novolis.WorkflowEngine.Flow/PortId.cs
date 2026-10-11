namespace Novolis.WorkflowEngine.Flow;

/// <summary>
/// Stable identity of a port within a node descriptor.
/// </summary>
public readonly record struct PortId : IComparable<PortId>
{
    /// <summary>
    /// Creates a port identity.
    /// </summary>
    public PortId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    /// <summary>The stable port identifier.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public int CompareTo(PortId other) =>
        string.Compare(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Value;
}
