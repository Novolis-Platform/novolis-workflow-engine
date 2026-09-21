namespace Novolis.WorkflowEngine;

/// <summary>
/// Executes the next stage of a workflow middleware chain.
/// </summary>
/// <param name="input">The current workflow payload.</param>
/// <param name="context">The current workflow context.</param>
/// <param name="cancellationToken">The run cancellation token.</param>
public delegate ValueTask<object?> WorkflowDelegate(
    object? input,
    WorkflowContext context,
    CancellationToken cancellationToken);
