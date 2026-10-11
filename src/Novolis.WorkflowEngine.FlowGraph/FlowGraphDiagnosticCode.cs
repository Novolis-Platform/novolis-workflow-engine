namespace Novolis.WorkflowEngine.FlowGraph;

/// <summary>
/// Machine-readable flow graph validation diagnostic code.
/// </summary>
public enum FlowGraphDiagnosticCode
{
    /// <summary>A node identity occurs more than once.</summary>
    DuplicateNodeId,

    /// <summary>A connection identity occurs more than once.</summary>
    DuplicateConnectionId,

    /// <summary>A connection source node is missing.</summary>
    MissingSourceNode,

    /// <summary>A connection destination node is missing.</summary>
    MissingDestinationNode,

    /// <summary>A connection source port is missing.</summary>
    MissingSourcePort,

    /// <summary>A connection destination port is missing.</summary>
    MissingDestinationPort,

    /// <summary>A connection source is not an output.</summary>
    SourceIsNotOutput,

    /// <summary>A connection destination is not an input.</summary>
    DestinationIsNotInput,

    /// <summary>A connection has incompatible semantic types.</summary>
    TypeMismatch,

    /// <summary>A connection exceeds a port cardinality.</summary>
    CardinalityExceeded,

    /// <summary>Two connections share the same node endpoints.</summary>
    DuplicateConnectionEndpoints,

    /// <summary>A connection is a self-loop.</summary>
    SelfLoop,

    /// <summary>The graph contains a cycle rejected by policy.</summary>
    Cycle,

    /// <summary>A node is inaccessible from the configured roots.</summary>
    InaccessibleNode,

    /// <summary>A subgraph dependency is recursive.</summary>
    RecursiveSubgraph,

    /// <summary>A subgraph identity occurs more than once.</summary>
    DuplicateSubgraphId,

    /// <summary>A referenced subgraph dependency is missing.</summary>
    MissingSubgraphDependency,

    /// <summary>A required input port has no connection.</summary>
    RequiredInputUnconnected,
}
