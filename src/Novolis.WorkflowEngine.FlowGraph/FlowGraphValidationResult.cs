using System.Collections.ObjectModel;
using Novolis.WorkflowEngine.Graph;

namespace Novolis.WorkflowEngine.FlowGraph;

/// <summary>
/// Result of validating a typed flow graph.
/// </summary>
public sealed class FlowGraphValidationResult
{
    internal FlowGraphValidationResult(
        GraphAnalysis topology,
        IReadOnlyList<FlowGraphDiagnostic> diagnostics)
    {
        Topology = topology;
        Diagnostics = new ReadOnlyCollection<FlowGraphDiagnostic>(
            diagnostics.ToArray());
    }

    /// <summary>Gets the underlying topology analysis.</summary>
    public GraphAnalysis Topology { get; }

    /// <summary>Gets all diagnostics in deterministic order.</summary>
    public IReadOnlyList<FlowGraphDiagnostic> Diagnostics { get; }

    /// <summary>Gets a value indicating whether the graph is valid.</summary>
    public bool IsValid => Diagnostics.All(diagnostic =>
        diagnostic.Severity != FlowGraphDiagnosticSeverity.Error);

    /// <summary>Gets a value indicating whether any errors were reported.</summary>
    public bool HasErrors => !IsValid;
}
