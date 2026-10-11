using System.Collections.ObjectModel;

namespace Novolis.WorkflowEngine.Flow;

/// <summary>
/// Versioned contract for a flow node kind.
/// </summary>
public sealed class FlowNodeDescriptor
{
    /// <summary>
    /// Creates a flow node descriptor.
    /// </summary>
    /// <param name="kind">Stable node kind identifier.</param>
    /// <param name="version">Version of the node contract.</param>
    /// <param name="ports">The node's typed ports.</param>
    public FlowNodeDescriptor(
        string kind,
        int version,
        IEnumerable<PortDescriptor> ports)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        if (version < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(version));
        }

        ArgumentNullException.ThrowIfNull(ports);
        var orderedPorts = ports
            .OrderBy(port => port.Id)
            .ToArray();
        if (orderedPorts.Any(port => port is null))
        {
            throw new ArgumentException("A node descriptor cannot contain a null port.", nameof(ports));
        }

        var duplicate = orderedPorts
            .GroupBy(port => port.Id)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new ArgumentException(
                $"Port identity '{duplicate.Key}' occurs more than once.",
                nameof(ports));
        }

        Kind = kind.Trim();
        Version = version;
        Ports = new ReadOnlyCollection<PortDescriptor>(orderedPorts);
    }

    /// <summary>The stable node kind identifier.</summary>
    public string Kind { get; }

    /// <summary>The node contract version.</summary>
    public int Version { get; }

    /// <summary>The node's typed ports.</summary>
    public IReadOnlyList<PortDescriptor> Ports { get; }

    /// <summary>The input ports.</summary>
    public IEnumerable<PortDescriptor> InputPorts =>
        Ports.Where(port => port.Direction == PortDirection.Input);

    /// <summary>The output ports.</summary>
    public IEnumerable<PortDescriptor> OutputPorts =>
        Ports.Where(port => port.Direction == PortDirection.Output);

    /// <summary>
    /// Tries to get a port descriptor by identity.
    /// </summary>
    public bool TryGetPort(PortId id, out PortDescriptor? port)
    {
        port = Ports.FirstOrDefault(candidate => candidate.Id == id);
        return port is not null;
    }
}
