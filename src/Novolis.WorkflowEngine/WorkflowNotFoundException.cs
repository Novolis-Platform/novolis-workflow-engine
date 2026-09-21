namespace Novolis.WorkflowEngine;

/// <summary>
/// Indicates that a requested workflow is not registered.
/// </summary>
public sealed class WorkflowNotFoundException : KeyNotFoundException
{
    /// <summary>
    /// Creates an exception for a missing workflow name.
    /// </summary>
    /// <param name="workflowName">The requested workflow name.</param>
    public WorkflowNotFoundException(string workflowName)
        : base($"Workflow '{workflowName}' is not registered.")
    {
        WorkflowName = workflowName;
    }

    /// <summary>The requested workflow name.</summary>
    public string WorkflowName { get; }
}
