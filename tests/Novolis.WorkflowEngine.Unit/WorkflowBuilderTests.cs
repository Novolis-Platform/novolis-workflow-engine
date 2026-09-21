using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Novolis.WorkflowEngine;

namespace Novolis.WorkflowEngine.Unit;

public sealed class WorkflowBuilderTests
{
    [Test]
    public async Task Workflow_runs_start_transform_and_end_steps()
    {
        var completion = new TaskCompletionSource<Output>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton(completion);
        builder.Services.AddWorkflow(workflow =>
        {
            workflow
                .StartWith<StartStep, Input>()
                .Then<TransformStep, Input, Output>()
                .ThenEndWith<EndStep, Output>();
        });

        using var host = builder.Build();
        await host.StartAsync();

        var result = await completion.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await Assert.That(result.Value).IsEqualTo("STARTED");
        await host.StopAsync();
    }

    [Test]
    public async Task Adjacent_steps_reuse_a_payload_channel()
    {
        var services = new ServiceCollection();

        services.AddWorkflow(workflow =>
        {
            workflow
                .StartWith<StartStep, Input>()
                .Then<IdentityStep, Input, Input>()
                .ThenEndWith<InputEndStep, Input>();
        });

        await Assert.That(services.Count(descriptor => descriptor.ServiceType == typeof(Channel<Input>)))
            .IsEqualTo(1);
    }

    [Test]
    public async Task Transform_step_requires_a_start_step()
    {
        var services = new ServiceCollection();

        await Assert.That(() => services.AddWorkflow(workflow =>
            workflow.Then<TransformStep, Input, Output>()))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Second_start_step_is_rejected()
    {
        var services = new ServiceCollection();

        await Assert.That(() => services.AddWorkflow(workflow =>
            workflow
                .StartWith<StartStep, Input>()
                .StartWith<StartStep, Input>()))
            .Throws<InvalidOperationException>();
    }

    private sealed record Input(string Value);

    private sealed record Output(string Value);

    private sealed class StartStep(ChannelWriter<Input> writer) : IStartStep<Input>
    {
        public Task RunAsync(CancellationToken cancellationToken) =>
            writer.WriteAsync(new Input("started"), cancellationToken).AsTask();
    }

    private sealed class TransformStep : IStep<Input, Output>
    {
        public Task<Output> ExecuteAsync(Input input) =>
            Task.FromResult(new Output(input.Value.ToUpperInvariant()));
    }

    private sealed class EndStep(TaskCompletionSource<Output> completion) : IEndStep<Output>
    {
        public Task ExecuteAsync(Output result)
        {
            completion.TrySetResult(result);
            return Task.CompletedTask;
        }
    }

    private sealed class IdentityStep : IStep<Input, Input>
    {
        public Task<Input> ExecuteAsync(Input input) => Task.FromResult(input);
    }

    private sealed class InputEndStep : IEndStep<Input>
    {
        public Task ExecuteAsync(Input result) => Task.CompletedTask;
    }
}
