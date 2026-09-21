using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Novolis.WorkflowEngine.Hosting;

/// <summary>
/// Pumps all registered workflow triggers into the host-independent engine.
/// </summary>
public sealed class WorkflowHostedService(
    WorkflowRegistry registry,
    IWorkflowEngine engine,
    IServiceScopeFactory scopeFactory,
    IOptions<WorkflowHostOptions> options,
    IHostApplicationLifetime lifetime,
    ILogger<WorkflowHostedService> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var definitions = registry.Definitions
            .Select(descriptor => registry.GetDefinition(descriptor.Name))
            .Where(definition => definition.HasTrigger)
            .ToArray();

        if (definitions.Length == 0)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken).ConfigureAwait(false);
            return;
        }

        var pumps = definitions
            .Select(definition => PumpAsync(definition, stoppingToken))
            .ToArray();

        try
        {
            await Task.WhenAll(pumps).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task PumpAsync(
        WorkflowDefinition definition,
        CancellationToken stoppingToken)
    {
        var maxConcurrency = Math.Max(1, options.Value.MaxConcurrentRunsPerWorkflow);
        var pending = new List<Task>(maxConcurrency);

        try
        {
            await using var triggerScope = scopeFactory.CreateAsyncScope();
            await foreach (var input in definition
                               .ReadAllAsync(triggerScope.ServiceProvider, stoppingToken)
                               .WithCancellation(stoppingToken)
                               .ConfigureAwait(false))
            {
                pending.Add(ExecuteOneAsync(definition, input, stoppingToken));

                if (pending.Count < maxConcurrency)
                {
                    continue;
                }

                var completed = await Task.WhenAny(pending).ConfigureAwait(false);
                pending.Remove(completed);
                await completed.ConfigureAwait(false);
            }

            await Task.WhenAll(pending).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Workflow trigger {WorkflowName} stopped unexpectedly.",
                definition.Name);
            lifetime.StopApplication();
        }
    }

    private async Task ExecuteOneAsync(
        WorkflowDefinition definition,
        object? input,
        CancellationToken stoppingToken)
    {
        var result = await engine.ExecuteAsync(
                definition.Name,
                input,
                stoppingToken)
            .ConfigureAwait(false);

        if (result.Succeeded ||
            (result.Status == WorkflowStatus.Canceled && stoppingToken.IsCancellationRequested))
        {
            return;
        }

        logger.LogError(
            result.Error,
            "Workflow {WorkflowName} run {RunId} ended with status {Status}.",
            result.WorkflowName,
            result.RunId,
            result.Status);

        if (options.Value.StopHostOnFailure)
        {
            lifetime.StopApplication();
        }
    }
}
