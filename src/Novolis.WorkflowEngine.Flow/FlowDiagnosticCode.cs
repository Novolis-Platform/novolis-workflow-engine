namespace Novolis.WorkflowEngine.Flow;

/// <summary>
/// Machine-readable flow connection diagnostic code.
/// </summary>
public enum FlowDiagnosticCode
{
    /// <summary>The source port is not an output.</summary>
    SourceIsNotOutput,

    /// <summary>The destination port is not an input.</summary>
    DestinationIsNotInput,

    /// <summary>The source and destination semantic types do not match.</summary>
    TypeMismatch,

    /// <summary>The destination port has reached its cardinality.</summary>
    CardinalityExceeded,

    /// <summary>The source and destination port identities are invalid.</summary>
    InvalidPort,
}
