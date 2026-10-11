using System.Collections.ObjectModel;
using Novolis.WorkflowEngine.Flow;

namespace Novolis.WorkflowEngine.FlowGraph;

/// <summary>
/// An immutable reference to a versioned reusable flow subgraph.
/// </summary>
public sealed class FlowSubgraphReference
{
    /// <summary>
    /// Creates a subgraph reference.
    /// </summary>
    /// <param name="id">The stable subgraph identity.</param>
    /// <param name="version">The subgraph contract version.</param>
    /// <param name="semanticFingerprint">The content fingerprint.</param>
    /// <param name="inputs">The exposed input ports.</param>
    /// <param name="outputs">The exposed output ports.</param>
    /// <param name="dependencies">Referenced subgraph identities.</param>
    public FlowSubgraphReference(
        string id,
        int version,
        string semanticFingerprint,
        IEnumerable<PortDescriptor>? inputs = null,
        IEnumerable<PortDescriptor>? outputs = null,
        IEnumerable<string>? dependencies = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(semanticFingerprint);
        if (version < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(version));
        }

        Id = id.Trim();
        Version = version;
        SemanticFingerprint = semanticFingerprint.Trim();
        Inputs = ToPorts(inputs);
        Outputs = ToPorts(outputs);
        Dependencies = new ReadOnlyCollection<string>(
            (dependencies ?? [])
                .Select(dependency =>
                {
                    ArgumentException.ThrowIfNullOrWhiteSpace(dependency);
                    return dependency.Trim();
                })
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray());
    }

    /// <summary>The stable subgraph identity.</summary>
    public string Id { get; }

    /// <summary>The exposed contract version.</summary>
    public int Version { get; }

    /// <summary>The immutable content fingerprint.</summary>
    public string SemanticFingerprint { get; }

    /// <summary>The exposed input ports.</summary>
    public IReadOnlyList<PortDescriptor> Inputs { get; }

    /// <summary>The exposed output ports.</summary>
    public IReadOnlyList<PortDescriptor> Outputs { get; }

    /// <summary>The referenced subgraph identities.</summary>
    public IReadOnlyList<string> Dependencies { get; }

    private static IReadOnlyList<PortDescriptor> ToPorts(
        IEnumerable<PortDescriptor>? ports)
    {
        var values = (ports ?? []).OrderBy(port => port.Id).ToArray();
        if (values.GroupBy(port => port.Id).Any(group => group.Count() > 1))
        {
            throw new ArgumentException("Exposed port identities must be unique.", nameof(ports));
        }

        return new ReadOnlyCollection<PortDescriptor>(values);
    }
}
