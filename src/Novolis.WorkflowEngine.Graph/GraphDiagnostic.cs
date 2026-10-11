namespace Novolis.WorkflowEngine.Graph;

/// <summary>
/// A diagnostic produced while analyzing graph topology.
/// </summary>
public sealed record GraphDiagnostic(
    GraphDiagnosticCode Code,
    GraphDiagnosticSeverity Severity,
    string Message,
    NodeId? NodeId = null,
    EdgeId? EdgeId = null);
