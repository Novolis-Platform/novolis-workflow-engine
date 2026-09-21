namespace Novolis.WorkflowEngine;

/// <summary>
/// Indicates that a workflow definition received an unexpected payload type.
/// </summary>
public sealed class WorkflowContractException : InvalidOperationException
{
    internal WorkflowContractException(
        string workflowName,
        Type expectedType,
        object? actualValue)
        : base(
            $"Workflow '{workflowName}' expected payload type '{expectedType.FullName}', " +
            $"but received '{actualValue?.GetType().FullName ?? "null"}'.")
    {
        WorkflowName = workflowName;
        ExpectedType = expectedType;
        ActualType = actualValue?.GetType();
    }

    /// <summary>The workflow name.</summary>
    public string WorkflowName { get; }

    /// <summary>The type required by the next workflow stage.</summary>
    public Type ExpectedType { get; }

    /// <summary>The type received by the next workflow stage.</summary>
    public Type? ActualType { get; }
}
