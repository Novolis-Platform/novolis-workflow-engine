using System.Collections.ObjectModel;

namespace Novolis.WorkflowEngine.Flow;

/// <summary>
/// A logical flow type independent of CLR runtime types.
/// </summary>
public sealed class FlowType : IEquatable<FlowType>
{
    /// <summary>
    /// Creates a semantic flow type.
    /// </summary>
    /// <param name="id">The stable logical type identifier.</param>
    /// <param name="arguments">Optional generic type arguments.</param>
    public FlowType(string id, IEnumerable<FlowType>? arguments = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Id = id.Trim();
        Arguments = new ReadOnlyCollection<FlowType>(
            (arguments ?? []).ToArray());
    }

    /// <summary>A wildcard type accepted by every type.</summary>
    public static FlowType Any { get; } = new("Any");

    /// <summary>A Boolean value.</summary>
    public static FlowType Bool { get; } = new("Bool");

    /// <summary>A scalar numeric value.</summary>
    public static FlowType Scalar { get; } = new("Scalar");

    /// <summary>An integer numeric value.</summary>
    public static FlowType Integer { get; } = new("Integer");

    /// <summary>A linear color value.</summary>
    public static FlowType Color { get; } = new("Color");

    /// <summary>A two-dimensional vector.</summary>
    public static FlowType Vector2 { get; } = new("Vector2");

    /// <summary>A three-dimensional vector.</summary>
    public static FlowType Vector3 { get; } = new("Vector3");

    /// <summary>Gets a field of another logical type.</summary>
    public static FlowType Field(FlowType elementType)
    {
        ArgumentNullException.ThrowIfNull(elementType);
        return new FlowType("Field", [elementType]);
    }

    /// <summary>Gets a list of another logical type.</summary>
    public static FlowType List(FlowType elementType)
    {
        ArgumentNullException.ThrowIfNull(elementType);
        return new FlowType("List", [elementType]);
    }

    /// <summary>The stable logical type identifier.</summary>
    public string Id { get; }

    /// <summary>The logical type arguments.</summary>
    public IReadOnlyList<FlowType> Arguments { get; }

    /// <summary>
    /// Determines whether this type can accept a source type.
    /// </summary>
    public bool IsCompatibleWith(FlowType source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return Id == "Any" || Equals(source);
    }

    /// <inheritdoc />
    public bool Equals(FlowType? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return other is not null &&
               string.Equals(Id, other.Id, StringComparison.Ordinal) &&
               Arguments.SequenceEqual(other.Arguments);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is FlowType other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Id, StringComparer.Ordinal);
        foreach (var argument in Arguments)
        {
            hash.Add(argument);
        }

        return hash.ToHashCode();
    }

    /// <inheritdoc />
    public override string ToString() =>
        Arguments.Count == 0
            ? Id
            : $"{Id}<{string.Join(", ", Arguments)}>";
}
