namespace Novolis.WorkflowEngine.Scheduling;

/// <summary>
/// Configuration for a cron-backed workflow trigger.
/// </summary>
/// <typeparam name="TPayload">The payload produced on each occurrence.</typeparam>
public sealed class CronWorkflowTriggerOptions<TPayload>
    where TPayload : class
{
    /// <summary>The six-field cron expression.</summary>
    public required string Expression { get; init; }

    /// <summary>Creates the payload for each occurrence.</summary>
    public required Func<TPayload> CreatePayload { get; init; }
}
