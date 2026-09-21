using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Novolis.WorkflowEngine;

internal sealed class StepRunner<TIn, TOut>(
    ChannelReader<TIn> reader,
    ChannelWriter<TOut> writer,
    IStep<TIn, TOut> action,
    ILogger<StepRunner<TIn, TOut>> logger) : BackgroundService
    where TIn : class
    where TOut : class
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var item in reader.ReadAllAsync(stoppingToken))
        {
            logger.LogInformation(
                "Workflow step {StepType} received input {Input}.",
                action.GetType().FullName,
                item);

            var output = await action.ExecuteAsync(item);
            await writer.WriteAsync(output, stoppingToken);
        }
    }
}
