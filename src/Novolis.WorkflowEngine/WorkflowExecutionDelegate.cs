namespace Novolis.WorkflowEngine;

internal delegate ValueTask<object?> WorkflowExecutionDelegate(
    IServiceProvider services,
    object? input,
    WorkflowContext context,
    CancellationToken cancellationToken);
