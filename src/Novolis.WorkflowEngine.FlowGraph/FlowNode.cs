using Novolis.WorkflowEngine.Flow;
using Novolis.WorkflowEngine.Graph;

namespace Novolis.WorkflowEngine.FlowGraph;

/// <summary>
/// A parameter-free flow graph node instance.
/// </summary>
public sealed record FlowNode
{
    /// <summary>
    /// Creates a flow node instance.
    /// </summary>
    /// <param name="id">The stable node identity.</param>
    /// <param name="descriptor">The versioned node contract.</param>
    public FlowNode(NodeId id, FlowNodeDescriptor descriptor)
    {
        if (id == NodeId.Empty)
        {
            throw new ArgumentException("A flow node must have a non-empty identity.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(descriptor);
        Id = id;
        Descriptor = descriptor;
    }

    /// <summary>The stable node identity.</summary>
    public NodeId Id { get; }

    /// <summary>The typed node contract.</summary>
    public FlowNodeDescriptor Descriptor { get; }

    /// <summary>The stable node kind identifier.</summary>
    public string Kind => Descriptor.Kind;

    /// <summary>The node contract version.</summary>
    public int Version => Descriptor.Version;
}
