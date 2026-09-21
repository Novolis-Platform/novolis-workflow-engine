using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Novolis.WorkflowEngine.Channels;
using Novolis.WorkflowEngine.Hosting;

namespace Novolis.WorkflowEngine.Unit;

public sealed class WorkflowHostingTests
{
    [Test]
    public async Task Channel_trigger_pumps_payloads_into_scoped_workflow_runs()
    {
        var completion = new TaskCompletionSource<Output>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton(completion);
        builder.Services.AddWorkflowChannelTrigger<Input>();
        builder.Services.AddWorkflow("channel-normalize", workflow => workflow
            .TriggeredBy<ChannelWorkflowTrigger<Input>, Input>()
            .Then<NormalizeStep, Input, Output>()
            .EndWith<CompletionSink, Output>());
        builder.Services.AddWorkflowHosting();

        using var host = builder.Build();
        await host.StartAsync();
        await host.Services
            .GetRequiredService<ChannelWriter<Input>>()
            .WriteAsync(new Input("from channel"));

        var result = await completion.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await host.StopAsync();

        await Assert.That(result.Value).IsEqualTo("FROM CHANNEL");
    }

    private sealed record Input(string Value);

    private sealed record Output(string Value);

    private sealed class NormalizeStep : IWorkflowStep<Input, Output>
    {
        public ValueTask<Output> ExecuteAsync(
            Input input,
            WorkflowContext context,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(new Output(input.Value.ToUpperInvariant()));
    }

    private sealed class CompletionSink(
        TaskCompletionSource<Output> completion) : IWorkflowSink<Output>
    {
        public ValueTask HandleAsync(
            Output payload,
            WorkflowContext context,
            CancellationToken cancellationToken = default)
        {
            completion.TrySetResult(payload);
            return ValueTask.CompletedTask;
        }
    }
}
