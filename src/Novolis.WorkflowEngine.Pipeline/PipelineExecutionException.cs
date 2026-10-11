namespace Novolis.WorkflowEngine.Pipeline;

/// <summary>
/// Indicates that a pipeline returned an explicit failure result.
/// </summary>
public sealed class PipelineExecutionException : Exception
{
    /// <summary>
    /// Creates an exception for an explicit pipeline failure.
    /// </summary>
    public PipelineExecutionException(Exception error)
        : base("The pipeline returned a failure result.", error)
    {
        ArgumentNullException.ThrowIfNull(error);
    }
}
