namespace Novolis.WorkflowEngine;

/// <summary>
/// Terminal state of a workflow execution.
/// </summary>
public enum WorkflowStatus
{
    /// <summary>The workflow completed successfully.</summary>
    Succeeded,

    /// <summary>The workflow failed with an exception.</summary>
    Failed,

    /// <summary>The workflow was canceled.</summary>
    Canceled
}
