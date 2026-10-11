using Novolis.WorkflowEngine.Flow;
using Novolis.WorkflowEngine.Graph;

namespace Novolis.WorkflowEngine.FlowGraph;

/// <summary>
/// Identifies one port on one flow graph node.
/// </summary>
public readonly record struct FlowPortReference
{
    /// <summary>
    /// Creates a flow port reference.
    /// </summary>
    public FlowPortReference(NodeId nodeId, PortId portId)
    {
        if (nodeId == NodeId.Empty)
        {
            throw new ArgumentException("A port reference needs a non-empty node identity.", nameof(nodeId));
        }

        if (string.IsNullOrWhiteSpace(portId.Value))
        {
            throw new ArgumentException("A port reference needs a non-empty port identity.", nameof(portId));
        }

        NodeId = nodeId;
        PortId = portId;
    }

    /// <summary>The node identity.</summary>
    public NodeId NodeId { get; }

    /// <summary>The port identity.</summary>
    public PortId PortId { get; }
}
