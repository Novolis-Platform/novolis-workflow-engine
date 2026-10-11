using System.Collections.ObjectModel;

namespace Novolis.WorkflowEngine.Pipeline;

/// <summary>
/// An immutable typed linear pipeline.
/// </summary>
/// <typeparam name="TInput">The initial input type.</typeparam>
/// <typeparam name="TOutput">The current output type.</typeparam>
public sealed class Pipeline<TInput, TOutput>
{
    private readonly ReadOnlyCollection<IPipelineStage> _stages;

    internal Pipeline(IEnumerable<IPipelineStage> stages)
    {
        ArgumentNullException.ThrowIfNull(stages);
        _stages = new ReadOnlyCollection<IPipelineStage>(stages.ToArray());
    }

    /// <summary>
    /// Adds a synchronous typed transformation.
    /// </summary>
    /// <typeparam name="TNext">The next output type.</typeparam>
    /// <param name="stage">The transformation.</param>
    public Pipeline<TInput, TNext> Then<TNext>(Func<TOutput, TNext> stage)
    {
        ArgumentNullException.ThrowIfNull(stage);
        return Append<TNext>(new SyncStage<TOutput, TNext>(stage));
    }

    /// <summary>
    /// Adds an asynchronous typed transformation.
    /// </summary>
    /// <typeparam name="TNext">The next output type.</typeparam>
    /// <param name="stage">The transformation.</param>
    public Pipeline<TInput, TNext> Then<TNext>(
        Func<TOutput, ValueTask<TNext>> stage)
    {
        ArgumentNullException.ThrowIfNull(stage);
        return Append<TNext>(new AsyncStage<TOutput, TNext>(
            (value, _) => stage(value)));
    }

    /// <summary>
    /// Adds an asynchronous typed transformation with cancellation.
    /// </summary>
    /// <typeparam name="TNext">The next output type.</typeparam>
    /// <param name="stage">The transformation.</param>
    public Pipeline<TInput, TNext> Then<TNext>(
        Func<TOutput, CancellationToken, ValueTask<TNext>> stage)
    {
        ArgumentNullException.ThrowIfNull(stage);
        return Append<TNext>(new AsyncStage<TOutput, TNext>(stage));
    }

    /// <summary>
    /// Adds a synchronous stage that can explicitly short-circuit with failure.
    /// </summary>
    /// <typeparam name="TNext">The next output type.</typeparam>
    /// <param name="stage">The result-producing transformation.</param>
    public Pipeline<TInput, TNext> ThenResult<TNext>(
        Func<TOutput, PipelineResult<TNext>> stage)
    {
        ArgumentNullException.ThrowIfNull(stage);
        return Append<TNext>(new ResultStage<TOutput, TNext>(
            (value, _) => ValueTask.FromResult(stage(value))));
    }

    /// <summary>
    /// Adds an asynchronous result stage that can explicitly short-circuit.
    /// </summary>
    /// <typeparam name="TNext">The next output type.</typeparam>
    /// <param name="stage">The result-producing transformation.</param>
    public Pipeline<TInput, TNext> ThenResult<TNext>(
        Func<TOutput, ValueTask<PipelineResult<TNext>>> stage)
    {
        ArgumentNullException.ThrowIfNull(stage);
        return Append<TNext>(new ResultStage<TOutput, TNext>(
            (value, _) => stage(value)));
    }

    /// <summary>
    /// Adds an asynchronous result stage with cancellation.
    /// </summary>
    /// <typeparam name="TNext">The next output type.</typeparam>
    /// <param name="stage">The result-producing transformation.</param>
    public Pipeline<TInput, TNext> ThenResult<TNext>(
        Func<TOutput, CancellationToken, ValueTask<PipelineResult<TNext>>> stage)
    {
        ArgumentNullException.ThrowIfNull(stage);
        return Append<TNext>(new ResultStage<TOutput, TNext>(stage));
    }

    /// <summary>
    /// Executes the pipeline and returns its final value.
    /// </summary>
    /// <param name="input">The initial input.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The final pipeline value.</returns>
    /// <exception cref="PipelineExecutionException">
    /// Thrown when an explicit result stage returns failure.
    /// </exception>
    public async ValueTask<TOutput> ExecuteAsync(
        TInput input,
        CancellationToken cancellationToken = default)
    {
        var result = await RunAsync(input, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            throw new PipelineExecutionException(result.Error!);
        }

        return (TOutput)result.Value!;
    }

    /// <summary>
    /// Executes the pipeline and converts exceptions or explicit failures to a result.
    /// </summary>
    /// <param name="input">The initial input.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async ValueTask<PipelineResult<TOutput>> ExecuteResultAsync(
        TInput input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await RunAsync(input, cancellationToken).ConfigureAwait(false);
            return result.IsSuccess
                ? PipelineResult<TOutput>.Success((TOutput)result.Value!)
                : PipelineResult<TOutput>.Failure(result.Error!);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return PipelineResult<TOutput>.Failure(exception);
        }
    }

    private Pipeline<TInput, TNext> Append<TNext>(IPipelineStage stage)
    {
        var stages = new IPipelineStage[_stages.Count + 1];
        _stages.CopyTo(stages, 0);
        stages[^1] = stage;
        return new Pipeline<TInput, TNext>(stages);
    }

    private async ValueTask<StageResult> RunAsync(
        TInput input,
        CancellationToken cancellationToken)
    {
        object? current = input;
        foreach (var stage in _stages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await stage.Invoke(current, cancellationToken)
                .ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                return result;
            }

            current = result.Value;
        }

        return new StageResult(true, current, null);
    }

    internal interface IPipelineStage
    {
        ValueTask<StageResult> Invoke(
            object? input,
            CancellationToken cancellationToken);
    }

    internal readonly record struct StageResult(
        bool IsSuccess,
        object? Value,
        Exception? Error);

    private sealed class SyncStage<TStageInput, TStageOutput>(
        Func<TStageInput, TStageOutput> stage) : IPipelineStage
    {
        public ValueTask<StageResult> Invoke(
            object? input,
            CancellationToken cancellationToken)
        {
            if (input is not TStageInput typedInput)
            {
                throw new PipelineContractException(typeof(TStageInput), input);
            }

            return ValueTask.FromResult(new StageResult(
                true,
                stage(typedInput),
                null));
        }
    }

    private sealed class AsyncStage<TStageInput, TStageOutput>(
        Func<TStageInput, CancellationToken, ValueTask<TStageOutput>> stage) : IPipelineStage
    {
        public async ValueTask<StageResult> Invoke(
            object? input,
            CancellationToken cancellationToken)
        {
            if (input is not TStageInput typedInput)
            {
                throw new PipelineContractException(typeof(TStageInput), input);
            }

            return new StageResult(
                true,
                await stage(typedInput, cancellationToken).ConfigureAwait(false),
                null);
        }
    }

    private sealed class ResultStage<TStageInput, TStageOutput>(
        Func<TStageInput, CancellationToken, ValueTask<PipelineResult<TStageOutput>>> stage)
        : IPipelineStage
    {
        public async ValueTask<StageResult> Invoke(
            object? input,
            CancellationToken cancellationToken)
        {
            if (input is not TStageInput typedInput)
            {
                throw new PipelineContractException(typeof(TStageInput), input);
            }

            var result = await stage(typedInput, cancellationToken).ConfigureAwait(false);
            ArgumentNullException.ThrowIfNull(result);
            return result.IsSuccess
                ? new StageResult(true, result.Value, null)
                : new StageResult(false, null, result.Error);
        }
    }
}
