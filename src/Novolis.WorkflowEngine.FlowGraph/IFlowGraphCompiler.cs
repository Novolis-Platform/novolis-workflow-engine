namespace Novolis.WorkflowEngine.FlowGraph;

/// <summary>
/// Compiles a validated flow graph into a consumer-owned plan.
/// </summary>
/// <typeparam name="TPlan">The compiler's target plan type.</typeparam>
public interface IFlowGraphCompiler<TPlan>
{
    /// <summary>
    /// Compiles a flow graph without executing node operations.
    /// </summary>
    /// <param name="graph">The graph to compile.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The consumer-owned plan.</returns>
    ValueTask<TPlan> CompileAsync(
        FlowGraph graph,
        CancellationToken cancellationToken = default);
}
