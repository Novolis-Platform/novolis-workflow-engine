using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Novolis.WorkflowEngine.Hosting;

/// <summary>
/// Generic-host integration for workflow triggers.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the background service that pumps registered workflow triggers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional hosting options.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddWorkflowHosting(
        this IServiceCollection services,
        Action<WorkflowHostOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddWorkflowEngine();

        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHostedService, WorkflowHostedService>());
        return services;
    }
}
