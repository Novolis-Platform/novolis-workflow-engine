namespace Novolis.WorkflowEngine.Pipeline;

internal interface IPipelineStage
{
    ValueTask<PipelineStageResult> Invoke(
        object? input,
        CancellationToken cancellationToken);
}
