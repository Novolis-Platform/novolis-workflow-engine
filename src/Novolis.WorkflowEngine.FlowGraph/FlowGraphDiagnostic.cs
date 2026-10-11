using Novolis.WorkflowEngine.Flow;
using Novolis.WorkflowEngine.Graph;

namespace Novolis.WorkflowEngine.FlowGraph;

/// <summary>
/// A diagnostic produced while validating a typed flow graph.
/// </summary>
public sealed record FlowGraphDiagnostic(
    FlowGraphDiagnosticCode Code,
    FlowGraphDiagnosticSeverity Severity,
    string Message,
    NodeId? NodeId = null,
    EdgeId? EdgeId = null,
    PortId? SourcePort = null,
    PortId? DestinationPort = null);
