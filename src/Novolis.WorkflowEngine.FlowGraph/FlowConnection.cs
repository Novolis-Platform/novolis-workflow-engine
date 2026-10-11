using Novolis.WorkflowEngine.Graph;

namespace Novolis.WorkflowEngine.FlowGraph;

/// <summary>
/// A typed connection between an output port and an input port.
/// </summary>
public sealed record FlowConnection
{
    /// <summary>
    /// Creates a flow connection.
    /// </summary>
    public FlowConnection(
        EdgeId id,
        FlowPortReference source,
        FlowPortReference destination)
    {
        if (id == EdgeId.Empty)
        {
            throw new ArgumentException("A flow connection must have a non-empty identity.", nameof(id));
        }

        Id = id;
        Source = source;
        Destination = destination;
    }

    /// <summary>The stable connection identity.</summary>
    public EdgeId Id { get; }

    /// <summary>The source output port.</summary>
    public FlowPortReference Source { get; }

    /// <summary>The destination input port.</summary>
    public FlowPortReference Destination { get; }
}
