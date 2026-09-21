using Microsoft.Extensions.DependencyInjection;
using Novolis.Mapping;
using Novolis.WorkflowEngine.Mapping;
using Novolis.WorkflowEngine.Scheduling;

namespace Novolis.WorkflowEngine.Unit;

public sealed class WorkflowAdaptersTests
{
    [Test]
    public async Task Mapping_adapter_composes_as_a_normal_step()
    {
        var completion = new TaskCompletionSource<Output>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var services = new ServiceCollection();
        services.AddSingleton(completion);
        services.AddScoped<UpperCaseMapping>();
        services.AddWorkflow("mapping", workflow => workflow
            .Accepts<Input>()
            .ThenMap<UpperCaseMapping, Input, Output>()
            .EndWith<CompletionSink, Output>());

        using var provider = services.BuildServiceProvider();
        var result = await provider
            .GetRequiredService<IWorkflowEngine>()
            .ExecuteAsync("mapping", new Input("mapped"));
        var output = await completion.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await Assert.That(result.Succeeded).IsTrue();
        await Assert.That(output.Value).IsEqualTo("MAPPED");
    }

    [Test]
    public async Task Cron_adapter_rejects_invalid_expression_at_registration()
    {
        var services = new ServiceCollection();

        await Assert.That(() => services.AddCronWorkflowTrigger(
                "not a cron",
                () => new Input("never")))
            .Throws<ArgumentException>();
    }

    private sealed record Input(string Value);

    private sealed record Output(string Value);

    private sealed class UpperCaseMapping : IMappingDefinition<Input, Output>
    {
        public Output Map(Input source) => new(source.Value.ToUpperInvariant());
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
