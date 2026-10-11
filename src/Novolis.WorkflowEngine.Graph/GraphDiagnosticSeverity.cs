namespace Novolis.WorkflowEngine.Graph;

/// <summary>
/// Severity of a graph diagnostic.
/// </summary>
public enum GraphDiagnosticSeverity
{
    /// <summary>Informational graph condition.</summary>
    Info,

    /// <summary>Non-fatal graph condition.</summary>
    Warning,

    /// <summary>Graph condition that prevents a consumer policy from accepting the graph.</summary>
    Error,
}
