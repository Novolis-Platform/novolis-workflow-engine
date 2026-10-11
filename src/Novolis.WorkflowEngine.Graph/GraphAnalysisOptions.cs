namespace Novolis.WorkflowEngine.Graph;

/// <summary>
/// Consumer policy used when analyzing a graph.
/// </summary>
public sealed record GraphAnalysisOptions
{
    /// <summary>
    /// Gets the default policy, which reports cycles without rejecting them.
    /// </summary>
    public static GraphAnalysisOptions Default { get; } = new();

    /// <summary>
    /// Gets a value indicating whether cycles produce errors.
    /// </summary>
    public bool RejectCycles { get; init; }

    /// <summary>
    /// Gets a value indicating whether self-loops produce errors.
    /// </summary>
    public bool RejectSelfLoops { get; init; }

    /// <summary>
    /// Gets a value indicating whether repeated ordered endpoints produce errors.
    /// </summary>
    public bool RejectDuplicateEdgeEndpoints { get; init; }

    /// <summary>
    /// Gets the roots used for reachability validation.
    /// </summary>
    public IReadOnlySet<NodeId>? Roots { get; init; }

    /// <summary>
    /// Gets a value indicating whether every graph node must be reachable from <see cref="Roots"/>.
    /// </summary>
    public bool RequireReachabilityFromRoots { get; init; }
}
