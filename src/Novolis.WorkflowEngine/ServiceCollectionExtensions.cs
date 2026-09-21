using Microsoft.Extensions.DependencyInjection;

namespace Novolis.WorkflowEngine;

/// <summary>
/// Workflow registration extensions for Microsoft dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds one workflow pipeline to the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">The workflow pipeline definition.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddWorkflow(
        this IServiceCollection services,
        Action<WorkflowBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new WorkflowBuilder(services);
        configure(builder);
        return services;
    }
}
