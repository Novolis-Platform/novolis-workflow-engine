namespace Novolis.WorkflowEngine;

/// <summary>
/// Transforms one workflow payload into another.
/// </summary>
/// <typeparam name="TInput">The input payload type.</typeparam>
/// <typeparam name="TOutput">The output payload type.</typeparam>
public interface IWorkflowStep<in TInput, TOutput>
    where TInput : class
    where TOutput : class
{
    /// <summary>
    /// Executes the transformation.
    /// </summary>
    /// <param name="input">The input payload.</param>
    /// <param name="context">The current workflow context.</param>
    /// <param name="cancellationToken">The run cancellation token.</param>
    /// <returns>The output payload.</returns>
    ValueTask<TOutput> ExecuteAsync(
        TInput input,
        WorkflowContext context,
        CancellationToken cancellationToken = default);
}
