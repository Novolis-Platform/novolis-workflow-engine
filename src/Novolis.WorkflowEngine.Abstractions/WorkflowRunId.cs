namespace Novolis.WorkflowEngine;

/// <summary>
/// Identifies one execution of a workflow.
/// </summary>
public readonly record struct WorkflowRunId(Guid Value)
{
    /// <summary>
    /// Creates a new run identifier.
    /// </summary>
    public static WorkflowRunId New() => new(Guid.NewGuid());

    /// <inheritdoc />
    public override string ToString() => Value.ToString("N");
}
