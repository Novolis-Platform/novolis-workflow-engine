namespace Novolis.WorkflowEngine;

/// <summary>
/// Produces workflow input values from any source: a channel, message bus,
/// timer, file watcher, or application callback.
/// </summary>
/// <typeparam name="TPayload">The payload type produced by the trigger.</typeparam>
public interface IWorkflowTrigger<out TPayload>
{
    /// <summary>
    /// Reads payloads until the source or host is canceled.
    /// </summary>
    /// <param name="cancellationToken">The host cancellation token.</param>
    /// <returns>An asynchronous stream of workflow payloads.</returns>
    IAsyncEnumerable<TPayload> ReadAllAsync(CancellationToken cancellationToken = default);
}
