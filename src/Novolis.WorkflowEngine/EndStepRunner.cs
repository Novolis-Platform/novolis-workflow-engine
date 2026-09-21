using System.Threading.Channels;
using Microsoft.Extensions.Hosting;

namespace Novolis.WorkflowEngine;

internal sealed class EndStepRunner<T>(
    ChannelReader<T> reader,
    IEndStep<T> action) : BackgroundService
    where T : class
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var item in reader.ReadAllAsync(stoppingToken))
        {
            await action.ExecuteAsync(item);
        }
    }
}
