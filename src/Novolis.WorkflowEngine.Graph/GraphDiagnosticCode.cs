namespace Novolis.WorkflowEngine.Graph;

/// <summary>
/// Machine-readable graph diagnostic code.
/// </summary>
public enum GraphDiagnosticCode
{
    /// <summary>A node identity occurs more than once.</summary>
    DuplicateNodeId,

    /// <summary>An edge identity occurs more than once.</summary>
    DuplicateEdgeId,

    /// <summary>An edge source does not identify a node in the graph.</summary>
    MissingSourceNode,

    /// <summary>An edge destination does not identify a node in the graph.</summary>
    MissingDestinationNode,

    /// <summary>Two edges connect the same ordered pair of nodes.</summary>
    DuplicateEdgeEndpoints,

    /// <summary>An edge connects a node to itself.</summary>
    SelfLoop,

    /// <summary>A strongly connected component contains a cycle.</summary>
    Cycle,

    /// <summary>A node cannot be reached from the configured root nodes.</summary>
    InaccessibleNode,
}
