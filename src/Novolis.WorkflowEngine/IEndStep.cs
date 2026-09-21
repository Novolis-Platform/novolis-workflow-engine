namespace Novolis.WorkflowEngine;

/// <summary>
/// Consumes the final payload in a workflow.
/// </summary>
/// <typeparam name="T">The final workflow payload type.</typeparam>
public interface IEndStep<in T>
    where T : class
{
    /// <summary>
    /// Consumes the workflow result.
    /// </summary>
    /// <param name="result">The final workflow payload.</param>
    /// <returns>A task that completes when the result has been handled.</returns>
    Task ExecuteAsync(T result);
}
