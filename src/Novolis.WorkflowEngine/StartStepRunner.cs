using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Novolis.WorkflowEngine;

internal sealed class StartStepRunner<T>(
    IStartStep<T> action,
    ILogger<StartStepRunner<T>> logger) : BackgroundService
    where T : class
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await action.RunAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Workflow start step {StepType} failed.", action.GetType().FullName);
            throw;
        }
    }
}
