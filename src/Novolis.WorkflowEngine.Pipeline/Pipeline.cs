namespace Novolis.WorkflowEngine.Pipeline;

/// <summary>
/// Entry point for creating typed linear pipelines.
/// </summary>
public static class Pipeline
{
    /// <summary>
    /// Starts a pipeline whose initial value has the specified type.
    /// </summary>
    /// <typeparam name="TInput">The initial value type.</typeparam>
    public static Pipeline<TInput, TInput> Start<TInput>() =>
        new([]);
}
