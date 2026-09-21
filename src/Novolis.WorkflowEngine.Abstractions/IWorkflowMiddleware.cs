namespace Novolis.WorkflowEngine;

/// <summary>
/// Cross-cutting behavior around a complete workflow execution.
/// </summary>
public interface IWorkflowMiddleware
{
    /// <summary>
    /// Runs middleware behavior around the next workflow stage.
    /// </summary>
    /// <param name="input">The current workflow payload.</param>
    /// <param name="context">The current workflow context.</param>
    /// <param name="next">The next stage in the middleware chain.</param>
    /// <param name="cancellationToken">The run cancellation token.</param>
    /// <returns>The payload returned by the next stage.</returns>
    ValueTask<object?> InvokeAsync(
        object? input,
        WorkflowContext context,
        WorkflowDelegate next,
        CancellationToken cancellationToken = default);
}
