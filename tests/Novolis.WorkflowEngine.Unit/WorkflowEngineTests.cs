using Microsoft.Extensions.DependencyInjection;
using Novolis.WorkflowEngine;

namespace Novolis.WorkflowEngine.Unit;

public sealed class WorkflowEngineTests
{
    [Test]
    public async Task Manual_workflow_runs_with_a_fresh_context()
    {
        var completion = new TaskCompletionSource<(Output Output, WorkflowContext Context)>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var services = new ServiceCollection();
        services.AddSingleton(completion);
        services.AddWorkflow("normalize", workflow => workflow
            .Accepts<Input>()
            .Use<RecordingMiddleware>()
            .Then<NormalizeStep, Input, Output>()
            .EndWith<CompletionSink, Output>());

        using var provider = services.BuildServiceProvider();
        var engine = provider.GetRequiredService<IWorkflowEngine>();
        var result = await engine.ExecuteAsync("normalize", new Input("hello"));
        var completed = await completion.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await Assert.That(result.Succeeded).IsTrue();
        await Assert.That(completed.Output.Value).IsEqualTo("HELLO");
        await Assert.That(completed.Context.RunId).IsEqualTo(result.RunId);
        await Assert.That(completed.Context.Get<string>("middleware")).IsEqualTo("seen");
        await Assert.That(result.Duration).IsGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    [Test]
    public async Task Failed_step_is_returned_as_a_result()
    {
        var services = new ServiceCollection();
        services.AddWorkflow("failing", workflow => workflow
            .Accepts<Input>()
            .Then<FailingStep, Input, Output>());

        using var provider = services.BuildServiceProvider();
        var result = await provider
            .GetRequiredService<IWorkflowEngine>()
            .ExecuteAsync("failing", new Input("boom"));

        await Assert.That(result.Status).IsEqualTo(WorkflowStatus.Failed);
        await Assert.That(result.Error).IsTypeOf<InvalidOperationException>();
        await Assert.That(() => result.ThrowIfFailed())
            .Throws<WorkflowExecutionException>();
    }

    [Test]
    public async Task Cancellation_is_reported_without_throwing()
    {
        var services = new ServiceCollection();
        services.AddWorkflow("cancellable", workflow => workflow
            .Accepts<Input>()
            .Then<CancellableStep, Input, Output>());

        using var provider = services.BuildServiceProvider();
        using var cancellation = new CancellationTokenSource();
        var execution = provider
            .GetRequiredService<IWorkflowEngine>()
            .ExecuteAsync("cancellable", new Input("wait"), cancellation.Token);

        cancellation.Cancel();
        var result = await execution;

        await Assert.That(result.Status).IsEqualTo(WorkflowStatus.Canceled);
    }

    [Test]
    public async Task Builder_rejects_mismatched_step_input()
    {
        var services = new ServiceCollection();

        await Assert.That(() => services.AddWorkflow("invalid", workflow => workflow
                .Accepts<Input>()
                .Then<OtherInputStep, OtherInput, Output>()))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Unknown_workflow_name_is_explicit()
    {
        var services = new ServiceCollection();
        services.AddWorkflowEngine();
        using var provider = services.BuildServiceProvider();

        await Assert.That(async () => await provider
                .GetRequiredService<IWorkflowEngine>()
                .ExecuteAsync("missing", new Input("missing")))
            .Throws<WorkflowNotFoundException>();
    }

    private sealed record Input(string Value);

    private sealed record OtherInput(string Value);

    private sealed record Output(string Value);

    private sealed class RecordingMiddleware : IWorkflowMiddleware
    {
        public async ValueTask<object?> InvokeAsync(
            object? input,
            WorkflowContext context,
            WorkflowDelegate next,
            CancellationToken cancellationToken = default)
        {
            context.Set("middleware", "seen");
            return await next(input, context, cancellationToken);
        }
    }

    private sealed class NormalizeStep : IWorkflowStep<Input, Output>
    {
        public ValueTask<Output> ExecuteAsync(
            Input input,
            WorkflowContext context,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(new Output(input.Value.ToUpperInvariant()));
    }

    private sealed class OtherInputStep : IWorkflowStep<OtherInput, Output>
    {
        public ValueTask<Output> ExecuteAsync(
            OtherInput input,
            WorkflowContext context,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(new Output(input.Value));
    }

    private sealed class CompletionSink(
        TaskCompletionSource<(Output Output, WorkflowContext Context)> completion)
        : IWorkflowSink<Output>
    {
        public ValueTask HandleAsync(
            Output payload,
            WorkflowContext context,
            CancellationToken cancellationToken = default)
        {
            completion.TrySetResult((payload, context));
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FailingStep : IWorkflowStep<Input, Output>
    {
        public ValueTask<Output> ExecuteAsync(
            Input input,
            WorkflowContext context,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("expected test failure");
    }

    private sealed class CancellableStep : IWorkflowStep<Input, Output>
    {
        public async ValueTask<Output> ExecuteAsync(
            Input input,
            WorkflowContext context,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return new Output(input.Value);
        }
    }
}
