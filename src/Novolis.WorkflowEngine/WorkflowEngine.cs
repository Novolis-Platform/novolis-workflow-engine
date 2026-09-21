using Microsoft.Extensions.DependencyInjection;

namespace Novolis.WorkflowEngine;

/// <summary>
/// Runs registered workflows with one scoped service graph per execution.
/// </summary>
public sealed class WorkflowEngine(
    WorkflowRegistry registry,
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider) : IWorkflowEngine
{
    /// <inheritdoc />
    public ValueTask<WorkflowExecutionResult> ExecuteAsync<TInput>(
        string workflowName,
        TInput input,
        CancellationToken cancellationToken = default)
        where TInput : class =>
        ExecuteAsync(workflowName, (object?)input, cancellationToken);

    /// <inheritdoc />
    public async ValueTask<WorkflowExecutionResult> ExecuteAsync(
        string workflowName,
        object? input,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowName);

        var definition = registry.GetDefinition(workflowName);
        var startedAt = timeProvider.GetUtcNow();
        var runId = WorkflowRunId.New();

        await using var scope = scopeFactory.CreateAsyncScope();
        var context = new WorkflowContext(
            definition.Name,
            runId,
            startedAt,
            scope.ServiceProvider);

        try
        {
            if (input is null || !definition.InputType.IsInstanceOfType(input))
            {
                throw new WorkflowContractException(
                    definition.Name,
                    definition.InputType,
                    input);
            }

            WorkflowDelegate pipeline = definition.ExecuteAsync;
            for (var index = definition.MiddlewareTypes.Count - 1; index >= 0; index--)
            {
                var next = pipeline;
                var middlewareType = definition.MiddlewareTypes[index];
                pipeline = (value, currentContext, token) =>
                {
                    var middleware =
                        (IWorkflowMiddleware)currentContext.Services.GetRequiredService(middlewareType);
                    return middleware.InvokeAsync(value, currentContext, next, token);
                };
            }

            await pipeline(input, context, cancellationToken).ConfigureAwait(false);
            var completedAt = timeProvider.GetUtcNow();
            return new WorkflowExecutionResult(
                definition.Name,
                runId,
                WorkflowStatus.Succeeded,
                startedAt,
                completedAt,
                null);
        }
        catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested)
        {
            return new WorkflowExecutionResult(
                definition.Name,
                runId,
                WorkflowStatus.Canceled,
                startedAt,
                timeProvider.GetUtcNow(),
                exception);
        }
        catch (Exception exception)
        {
            return new WorkflowExecutionResult(
                definition.Name,
                runId,
                WorkflowStatus.Failed,
                startedAt,
                timeProvider.GetUtcNow(),
                exception);
        }
    }
}
