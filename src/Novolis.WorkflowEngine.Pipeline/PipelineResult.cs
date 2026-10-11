namespace Novolis.WorkflowEngine.Pipeline;

/// <summary>
/// Explicit success or failure returned by a pipeline stage.
/// </summary>
/// <typeparam name="T">The successful value type.</typeparam>
public sealed record PipelineResult<T>
{
    private PipelineResult(T? value, Exception? error, bool isSuccess)
    {
        Value = value;
        Error = error;
        IsSuccess = isSuccess;
    }

    /// <summary>Gets a successful result.</summary>
    public static PipelineResult<T> Success(T value) =>
        new(value, null, true);

    /// <summary>Gets a failed result.</summary>
    public static PipelineResult<T> Failure(Exception error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(default, error, false);
    }

    /// <summary>Gets a failed result with an explanatory message.</summary>
    public static PipelineResult<T> Failure(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        return Failure(new InvalidOperationException(message));
    }

    /// <summary>Gets a value indicating whether the result succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>The successful value, or <c>default</c> on failure.</summary>
    public T? Value { get; }

    /// <summary>The failure, or <c>null</c> on success.</summary>
    public Exception? Error { get; }

    /// <summary>
    /// Gets the successful value or throws for a failed result.
    /// </summary>
    public T GetValueOrThrow()
    {
        if (IsSuccess)
        {
            return Value!;
        }

        throw new PipelineExecutionException(Error!);
    }
}
