namespace Novolis.WorkflowEngine;

/// <summary>
/// Outcome and timing data for one workflow execution.
/// </summary>
public sealed record WorkflowExecutionResult
{
    /// <summary>
    /// Creates an execution result.
    /// </summary>
    /// <param name="workflowName">The workflow name.</param>
    /// <param name="runId">The run identifier.</param>
    /// <param name="status">The terminal status.</param>
    /// <param name="startedAt">The start time.</param>
    /// <param name="completedAt">The completion time.</param>
    /// <param name="error">The execution error, when failed.</param>
    public WorkflowExecutionResult(
        string workflowName,
        WorkflowRunId runId,
        WorkflowStatus status,
        DateTimeOffset startedAt,
        DateTimeOffset completedAt,
        Exception? error)
    {
        WorkflowName = workflowName;
        RunId = runId;
        Status = status;
        StartedAt = startedAt;
        CompletedAt = completedAt;
        Error = error;
    }

    /// <summary>The registered workflow name.</summary>
    public string WorkflowName { get; }

    /// <summary>The identifier for this execution.</summary>
    public WorkflowRunId RunId { get; }

    /// <summary>The terminal workflow status.</summary>
    public WorkflowStatus Status { get; }

    /// <summary>The UTC time at which execution started.</summary>
    public DateTimeOffset StartedAt { get; }

    /// <summary>The UTC time at which execution completed.</summary>
    public DateTimeOffset CompletedAt { get; }

    /// <summary>The failure that ended execution, when <see cref="Status"/> is <see cref="WorkflowStatus.Failed"/>.</summary>
    public Exception? Error { get; }

    /// <summary>The elapsed execution time.</summary>
    public TimeSpan Duration => CompletedAt - StartedAt;

    /// <summary>Whether the workflow completed successfully.</summary>
    public bool Succeeded => Status == WorkflowStatus.Succeeded;

    /// <summary>
    /// Throws a descriptive exception when this result is not successful.
    /// </summary>
    public void ThrowIfFailed()
    {
        if (Succeeded)
        {
            return;
        }

        throw new WorkflowExecutionException(this);
    }
}
