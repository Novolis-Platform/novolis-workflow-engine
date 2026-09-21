using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Novolis.Messaging.Channels;

namespace Novolis.WorkflowEngine.Channels;

/// <summary>
/// Registers channel-backed workflow triggers.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a channel and its workflow trigger adapter.
    /// </summary>
    /// <typeparam name="TPayload">The channel payload type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddWorkflowChannelTrigger<TPayload>(
        this IServiceCollection services)
        where TPayload : class
    {
        ArgumentNullException.ThrowIfNull(services);

        if (!services.Any(descriptor => descriptor.ServiceType == typeof(Channel<TPayload>)))
        {
            services.AddChannel<TPayload>();
        }

        services.TryAddScoped<ChannelWorkflowTrigger<TPayload>>();
        return services;
    }
}
