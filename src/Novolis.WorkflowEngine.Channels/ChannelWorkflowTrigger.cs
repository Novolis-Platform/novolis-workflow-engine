using System.Threading.Channels;

namespace Novolis.WorkflowEngine.Channels;

/// <summary>
/// Adapts a channel reader into a workflow trigger.
/// </summary>
/// <typeparam name="TPayload">The channel payload type.</typeparam>
public sealed class ChannelWorkflowTrigger<TPayload>(
    ChannelReader<TPayload> reader) : IWorkflowTrigger<TPayload>
    where TPayload : class
{
    /// <inheritdoc />
    public IAsyncEnumerable<TPayload> ReadAllAsync(
        CancellationToken cancellationToken = default) =>
        reader.ReadAllAsync(cancellationToken);
}
