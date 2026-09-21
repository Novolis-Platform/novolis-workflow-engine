using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Novolis.WorkflowEngine;

/// <summary>
/// Registers workflow definitions and the host-independent workflow engine.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the engine services without defining a workflow.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddWorkflowEngine(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<WorkflowRegistry>();
        services.TryAddSingleton<IWorkflowRegistry>(provider =>
            provider.GetRequiredService<WorkflowRegistry>());
        services.TryAddSingleton<IWorkflowEngine, WorkflowEngine>();
        return services;
    }

    /// <summary>
    /// Defines one named workflow.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The stable workflow name.</param>
    /// <param name="configure">The fluent workflow definition.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddWorkflow(
        this IServiceCollection services,
        string name,
        Action<WorkflowBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new WorkflowBuilder(services, name);
        configure(builder);
        services.AddSingleton(builder.Build());
        return services.AddWorkflowEngine();
    }
}
