using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Novolis.Scheduling.Cron;

namespace Novolis.WorkflowEngine.Scheduling;

/// <summary>
/// Registers cron-backed workflow triggers.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds one cron trigger configuration for a payload type.
    /// </summary>
    /// <typeparam name="TPayload">The payload type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="expression">The six-field cron expression.</param>
    /// <param name="createPayload">The payload factory.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddCronWorkflowTrigger<TPayload>(
        this IServiceCollection services,
        string expression,
        Func<TPayload> createPayload)
        where TPayload : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        ArgumentNullException.ThrowIfNull(createPayload);

        if (!CronHelper.IsValid(expression))
        {
            throw new ArgumentException(
                $"'{expression}' is not a valid cron expression.",
                nameof(expression));
        }

        services.AddSingleton(new CronWorkflowTriggerOptions<TPayload>
        {
            Expression = expression,
            CreatePayload = createPayload
        });
        services.TryAddScoped<CronWorkflowTrigger<TPayload>>();
        return services;
    }
}
