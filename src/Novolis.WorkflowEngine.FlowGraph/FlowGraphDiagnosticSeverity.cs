namespace Novolis.WorkflowEngine.FlowGraph;

/// <summary>
/// Severity of a flow graph validation diagnostic.
/// </summary>
public enum FlowGraphDiagnosticSeverity
{
    /// <summary>Informational condition.</summary>
    Info,

    /// <summary>Non-fatal condition.</summary>
    Warning,

    /// <summary>Condition that prevents validation.</summary>
    Error,
}
