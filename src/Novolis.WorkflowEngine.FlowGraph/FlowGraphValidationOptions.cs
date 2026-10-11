namespace Novolis.WorkflowEngine.FlowGraph;

/// <summary>
/// Validation policy for a typed flow graph.
/// </summary>
public sealed record FlowGraphValidationOptions
{
    /// <summary>Policy for compile-only visual/data graphs.</summary>
    public static FlowGraphValidationOptions DagOnly { get; } = new()
    {
        RejectCycles = true,
        RejectRecursiveSubgraphs = true,
    };

    /// <summary>Policy that permits graph cycles and recursive subgraph references.</summary>
    public static FlowGraphValidationOptions AllowCycles { get; } = new()
    {
        RejectCycles = false,
        RejectRecursiveSubgraphs = false,
    };

    /// <summary>Gets a value indicating whether graph cycles are errors.</summary>
    public bool RejectCycles { get; init; } = true;

    /// <summary>Gets a value indicating whether recursive subgraphs are errors.</summary>
    public bool RejectRecursiveSubgraphs { get; init; } = true;

    /// <summary>Gets a value indicating whether all required inputs must be connected.</summary>
    public bool RequireAllInputsConnected { get; init; }
}
