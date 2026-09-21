using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Novolis.Messaging.Channels;

namespace Novolis.WorkflowEngine;

/// <summary>
/// Registers a linear workflow pipeline in an <see cref="IServiceCollection"/>.
/// </summary>
/// <param name="services">The service collection that owns the workflow.</param>
public sealed class WorkflowBuilder(IServiceCollection services)
{
    private bool _hasStartStep;

    /// <summary>
    /// Registers the step that produces the first payload when the host starts.
    /// </summary>
    /// <typeparam name="TStep">The start-step implementation.</typeparam>
    /// <typeparam name="TOut">The first payload type.</typeparam>
    /// <returns>This builder.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a start step has already been registered.
    /// </exception>
    public WorkflowBuilder StartWith<TStep, TOut>()
        where TStep : class, IStartStep<TOut>
        where TOut : class
    {
        if (_hasStartStep)
        {
            throw new InvalidOperationException("Start step already defined.");
        }

        _hasStartStep = true;
        AddChannelIfMissing<TOut>();
        services.AddSingleton<IStartStep<TOut>, TStep>();
        services.AddHostedService<StartStepRunner<TOut>>();
        return this;
    }

    /// <summary>
    /// Registers a transform step between two payload types.
    /// </summary>
    /// <typeparam name="TStep">The transform-step implementation.</typeparam>
    /// <typeparam name="TIn">The input payload type.</typeparam>
    /// <typeparam name="TOut">The output payload type.</typeparam>
    /// <returns>This builder.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a start step has not yet been registered.
    /// </exception>
    public WorkflowBuilder Then<TStep, TIn, TOut>()
        where TStep : class, IStep<TIn, TOut>
        where TIn : class
        where TOut : class
    {
        EnsureStartStep();
        services.AddSingleton<IStep<TIn, TOut>, TStep>();
        AddChannelIfMissing<TIn>();
        AddChannelIfMissing<TOut>();
        services.AddHostedService<StepRunner<TIn, TOut>>();
        return this;
    }

    /// <summary>
    /// Registers the final step in the workflow.
    /// </summary>
    /// <typeparam name="TStep">The end-step implementation.</typeparam>
    /// <typeparam name="TIn">The final payload type.</typeparam>
    /// <returns>This builder.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a start step has not yet been registered.
    /// </exception>
    public WorkflowBuilder ThenEndWith<TStep, TIn>()
        where TStep : class, IEndStep<TIn>
        where TIn : class
    {
        EnsureStartStep();
        services.AddSingleton<IEndStep<TIn>, TStep>();
        AddChannelIfMissing<TIn>();
        services.AddHostedService<EndStepRunner<TIn>>();
        return this;
    }

    private void AddChannelIfMissing<T>()
        where T : class
    {
        if (services.Any(descriptor => descriptor.ServiceType == typeof(Channel<T>)))
        {
            return;
        }

        services.AddChannel<T>();
    }

    private void EnsureStartStep()
    {
        if (!_hasStartStep)
        {
            throw new InvalidOperationException("Start step not defined.");
        }
    }
}
