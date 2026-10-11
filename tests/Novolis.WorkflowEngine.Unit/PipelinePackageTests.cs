using Novolis.WorkflowEngine.Pipeline;

namespace Novolis.WorkflowEngine.Unit;

public sealed class PipelinePackageTests
{
    [Test]
    public async Task Typed_stages_compose_in_order()
    {
        var pipeline = Pipeline
            .Start<string>()
            .Then(value => value.Trim())
            .Then(value => value.ToUpperInvariant())
            .Then(value => value.Length);

        var output = await pipeline.ExecuteAsync(" hello ");

        await Assert.That(output).IsEqualTo(5);
    }

    [Test]
    public async Task ValueTask_stage_receives_cancellation()
    {
        var pipeline = Pipeline
            .Start<int>()
            .Then<int>((value, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return ValueTask.FromResult(value + 1);
            });
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.That(async () =>
            await pipeline.ExecuteAsync(1, cancellation.Token))
            .Throws<OperationCanceledException>();
    }

    [Test]
    public async Task Explicit_result_stage_short_circuits()
    {
        var executed = false;
        var pipeline = Pipeline
            .Start<string>()
            .ThenResult(value => PipelineResult<int>.Failure("invalid"))
            .Then(value =>
            {
                executed = true;
                return value + 1;
            });

        var result = await pipeline.ExecuteResultAsync("value");

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Error).IsTypeOf<InvalidOperationException>();
        await Assert.That(executed).IsFalse();
    }
}
