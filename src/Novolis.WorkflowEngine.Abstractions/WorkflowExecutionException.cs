namespace Novolis.WorkflowEngine;

/// <summary>
/// Describes a workflow execution that failed or was canceled.
/// </summary>
public sealed class WorkflowExecutionException : Exception
{
    /// <summary>
    /// Creates an exception from an execution result.
    /// </summary>
    /// <param name="result">The unsuccessful execution result.</param>
    public WorkflowExecutionException(WorkflowExecutionResult result)
        : base(CreateMessage(result), result.Error)
    {
        Result = result;
    }

    /// <summary>The unsuccessful execution result.</summary>
    public WorkflowExecutionResult Result { get; }

    private static string CreateMessage(WorkflowExecutionResult result) =>
        result.Status == WorkflowStatus.Canceled
            ? $"Workflow '{result.WorkflowName}' run {result.RunId} was canceled."
            : $"Workflow '{result.WorkflowName}' run {result.RunId} failed.";
}
