using System.Collections.ObjectModel;

namespace Novolis.WorkflowEngine.FlowGraph;

/// <summary>
/// Result of attempting to add one flow graph connection.
/// </summary>
public sealed class FlowGraphConnectionResult
{
    internal FlowGraphConnectionResult(
        bool isValid,
        FlowGraph graph,
        IReadOnlyList<FlowGraphDiagnostic> diagnostics)
    {
        IsValid = isValid;
        Graph = graph;
        Diagnostics = new ReadOnlyCollection<FlowGraphDiagnostic>(
            diagnostics.ToArray());
    }

    /// <summary>Gets a value indicating whether the connection was accepted.</summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the resulting graph. This is the original graph when the attempt fails.
    /// </summary>
    public FlowGraph Graph { get; }

    /// <summary>Gets diagnostics explaining a rejected connection.</summary>
    public IReadOnlyList<FlowGraphDiagnostic> Diagnostics { get; }
}
