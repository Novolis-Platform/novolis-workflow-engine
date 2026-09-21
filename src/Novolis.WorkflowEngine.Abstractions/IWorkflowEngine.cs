namespace Novolis.WorkflowEngine;

/// <summary>
/// Executes registered workflows on demand.
/// </summary>
public interface IWorkflowEngine
{
    /// <summary>
    /// Executes a named workflow with one input payload.
    /// </summary>
    /// <param name="workflowName">The registered workflow name.</param>
    /// <param name="input">The workflow input payload.</param>
    /// <param name="cancellationToken">The run cancellation token.</param>
    /// <returns>Outcome and timing data for the run.</returns>
    ValueTask<WorkflowExecutionResult> ExecuteAsync(
        string workflowName,
        object? input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a named workflow with a strongly typed input payload.
    /// </summary>
    /// <typeparam name="TInput">The workflow input type.</typeparam>
    /// <param name="workflowName">The registered workflow name.</param>
    /// <param name="input">The workflow input payload.</param>
    /// <param name="cancellationToken">The run cancellation token.</param>
    /// <returns>Outcome and timing data for the run.</returns>
    ValueTask<WorkflowExecutionResult> ExecuteAsync<TInput>(
        string workflowName,
        TInput input,
        CancellationToken cancellationToken = default)
        where TInput : class;
}
