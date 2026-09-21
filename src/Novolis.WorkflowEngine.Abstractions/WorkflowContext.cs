namespace Novolis.WorkflowEngine;

/// <summary>
/// Per-run metadata shared by workflow components.
/// </summary>
public sealed class WorkflowContext
{
    private readonly Dictionary<string, object?> _items = new(StringComparer.Ordinal);

    /// <summary>
    /// Creates a workflow context.
    /// </summary>
    /// <param name="workflowName">The workflow name.</param>
    /// <param name="runId">The execution identifier.</param>
    /// <param name="startedAt">The execution start time.</param>
    public WorkflowContext(
        string workflowName,
        WorkflowRunId runId,
        DateTimeOffset startedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowName);
        WorkflowName = workflowName;
        RunId = runId;
        StartedAt = startedAt;
    }

    /// <summary>The registered workflow name.</summary>
    public string WorkflowName { get; }

    /// <summary>The identifier for this execution.</summary>
    public WorkflowRunId RunId { get; }

    /// <summary>The UTC time at which this execution started.</summary>
    public DateTimeOffset StartedAt { get; }

    /// <summary>Mutable values shared by steps in this execution.</summary>
    public IDictionary<string, object?> Items => _items;

    /// <summary>
    /// Gets a value previously stored in the context.
    /// </summary>
    /// <typeparam name="T">The expected value type.</typeparam>
    /// <param name="key">The item key.</param>
    /// <returns>The stored value, or <c>default</c> when absent.</returns>
    public T? Get<T>(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _items.TryGetValue(key, out var value) && value is T typed ? typed : default;
    }

    /// <summary>
    /// Stores a value in the execution context.
    /// </summary>
    /// <param name="key">The item key.</param>
    /// <param name="value">The value to store.</param>
    public void Set(string key, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        _items[key] = value;
    }
}
