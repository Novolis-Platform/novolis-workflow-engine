using System.Collections.ObjectModel;

namespace Novolis.WorkflowEngine.Graph;

/// <summary>
/// Deterministic structural analysis of a graph snapshot.
/// </summary>
public sealed class GraphAnalysis
{
    internal GraphAnalysis(
        IReadOnlyList<GraphDiagnostic> diagnostics,
        IReadOnlyList<IReadOnlyList<NodeId>> stronglyConnectedComponents,
        IReadOnlyList<NodeId> topologicalOrder,
        bool isAcyclic)
    {
        Diagnostics = new ReadOnlyCollection<GraphDiagnostic>(diagnostics.ToArray());
        StronglyConnectedComponents = new ReadOnlyCollection<IReadOnlyList<NodeId>>(
            stronglyConnectedComponents
                .Select(component => (IReadOnlyList<NodeId>)new ReadOnlyCollection<NodeId>(
                    component.ToArray()))
                .ToArray());
        TopologicalOrder = new ReadOnlyCollection<NodeId>(topologicalOrder.ToArray());
        IsAcyclic = isAcyclic;
    }

    /// <summary>Gets all diagnostics in stable order.</summary>
    public IReadOnlyList<GraphDiagnostic> Diagnostics { get; }

    /// <summary>Gets strongly connected components in stable order.</summary>
    public IReadOnlyList<IReadOnlyList<NodeId>> StronglyConnectedComponents { get; }

    /// <summary>
    /// Gets a deterministic topological order, or an empty list when the graph
    /// cannot be topologically ordered.
    /// </summary>
    public IReadOnlyList<NodeId> TopologicalOrder { get; }

    /// <summary>Gets a value indicating whether the graph has no cycles.</summary>
    public bool IsAcyclic { get; }

    /// <summary>Gets a value indicating whether the graph satisfies the selected policy.</summary>
    public bool IsValid => Diagnostics.All(diagnostic =>
        diagnostic.Severity != GraphDiagnosticSeverity.Error);

    /// <summary>Gets a value indicating whether any errors were reported.</summary>
    public bool HasErrors => !IsValid;

    /// <summary>Gets a value indicating whether any warnings were reported.</summary>
    public bool HasWarnings => Diagnostics.Any(diagnostic =>
        diagnostic.Severity == GraphDiagnosticSeverity.Warning);
}
