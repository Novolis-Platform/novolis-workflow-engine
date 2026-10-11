namespace Novolis.WorkflowEngine.Pipeline;

/// <summary>
/// Indicates that a pipeline stage received a value of an unexpected type.
/// </summary>
public sealed class PipelineContractException : Exception
{
    /// <summary>
    /// Creates a pipeline contract exception.
    /// </summary>
    public PipelineContractException(Type expectedType, object? actualValue)
        : base(
            $"The pipeline expected '{expectedType.FullName}' but received " +
            $"'{actualValue?.GetType().FullName ?? "null"}'.")
    {
        ExpectedType = expectedType;
        ActualType = actualValue?.GetType();
    }

    /// <summary>The expected value type.</summary>
    public Type ExpectedType { get; }

    /// <summary>The actual value type, or <c>null</c>.</summary>
    public Type? ActualType { get; }
}
