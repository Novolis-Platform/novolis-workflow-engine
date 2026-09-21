namespace Novolis.WorkflowEngine;

/// <summary>
/// Consumes the final payload of a workflow.
/// </summary>
/// <typeparam name="TPayload">The final payload type.</typeparam>
public interface IWorkflowSink<in TPayload>
    where TPayload : class
{
    /// <summary>
    /// Handles the final workflow payload.
    /// </summary>
    /// <param name="payload">The final payload.</param>
    /// <param name="context">The current workflow context.</param>
    /// <param name="cancellationToken">The run cancellation token.</param>
    /// <returns>A task that completes after the payload has been handled.</returns>
    ValueTask HandleAsync(
        TPayload payload,
        WorkflowContext context,
        CancellationToken cancellationToken = default);
}
