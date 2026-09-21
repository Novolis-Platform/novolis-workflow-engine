using Novolis.Scheduling.Cron;

namespace Novolis.WorkflowEngine.Scheduling;

/// <summary>
/// Emits payloads on a Novolis cron schedule.
/// </summary>
/// <typeparam name="TPayload">The payload type.</typeparam>
public sealed class CronWorkflowTrigger<TPayload>(
    CronWorkflowTriggerOptions<TPayload> options,
    TimeProvider timeProvider) : IWorkflowTrigger<TPayload>
    where TPayload : class
{
    /// <inheritdoc />
    public async IAsyncEnumerable<TPayload> ReadAllAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        _ = CronHelper.Parse(options.Expression);

        while (!cancellationToken.IsCancellationRequested)
        {
            var now = timeProvider.GetUtcNow().UtcDateTime;
            var next = CronHelper.GetNextOccurrence(options.Expression, now);
            var delay = next - now;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }

            yield return options.CreatePayload();
        }
    }
}
