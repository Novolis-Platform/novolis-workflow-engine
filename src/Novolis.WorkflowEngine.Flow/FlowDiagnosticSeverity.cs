namespace Novolis.WorkflowEngine.Flow;

/// <summary>
/// Severity of a flow connection diagnostic.
/// </summary>
public enum FlowDiagnosticSeverity
{
    /// <summary>Informational diagnostic.</summary>
    Info,

    /// <summary>Non-fatal diagnostic.</summary>
    Warning,

    /// <summary>Connection cannot be accepted.</summary>
    Error,
}
