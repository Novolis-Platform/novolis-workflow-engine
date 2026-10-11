namespace Novolis.WorkflowEngine.Pipeline;

internal readonly record struct PipelineStageResult(
    bool IsSuccess,
    object? Value,
    Exception? Error);
