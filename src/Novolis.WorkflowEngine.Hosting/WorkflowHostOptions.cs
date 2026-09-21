namespace Novolis.WorkflowEngine.Hosting;

/// <summary>
/// Controls generic-host execution of workflow triggers.
/// </summary>
public sealed class WorkflowHostOptions
{
    /// <summary>
    /// Maximum number of payloads from one trigger that may execute concurrently.
    /// </summary>
    public int MaxConcurrentRunsPerWorkflow { get; set; } = 1;

    /// <summary>
    /// Whether a failed workflow run should request host shutdown.
    /// </summary>
    public bool StopHostOnFailure { get; set; }
}
