namespace Novolis.WorkflowEngine.Flow;

/// <summary>
/// A diagnostic from flow contract validation.
/// </summary>
public sealed record FlowDiagnostic(
    FlowDiagnosticCode Code,
    FlowDiagnosticSeverity Severity,
    string Message,
    PortId? SourcePort = null,
    PortId? DestinationPort = null);
