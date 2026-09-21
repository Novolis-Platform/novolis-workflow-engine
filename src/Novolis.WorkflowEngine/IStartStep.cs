namespace Novolis.WorkflowEngine;

/// <summary>
/// Produces the first value in a workflow when the host starts.
/// </summary>
/// <typeparam name="TOut">The first workflow payload type.</typeparam>
public interface IStartStep<TOut>
    where TOut : class
{
    /// <summary>
    /// Produces workflow input until the host is stopping.
    /// </summary>
    /// <param name="cancellationToken">The host shutdown token.</param>
    /// <returns>A task that completes when this start invocation is finished.</returns>
    Task RunAsync(CancellationToken cancellationToken);
}
