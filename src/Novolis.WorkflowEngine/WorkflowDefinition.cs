namespace Novolis.WorkflowEngine;

/// <summary>
/// An immutable workflow definition registered with the engine.
/// </summary>
public sealed class WorkflowDefinition
{
    private readonly WorkflowExecutionDelegate _pipeline;
    private readonly Func<IServiceProvider, CancellationToken, IAsyncEnumerable<object?>>? _trigger;

    internal WorkflowDefinition(
        WorkflowDefinitionDescriptor descriptor,
        WorkflowExecutionDelegate pipeline,
        Func<IServiceProvider, CancellationToken, IAsyncEnumerable<object?>>? trigger,
        IReadOnlyList<Type> middlewareTypes)
    {
        Descriptor = descriptor;
        _pipeline = pipeline;
        _trigger = trigger;
        MiddlewareTypes = middlewareTypes;
    }

    /// <summary>Public metadata for this workflow.</summary>
    public WorkflowDefinitionDescriptor Descriptor { get; }

    /// <summary>The registered workflow name.</summary>
    public string Name => Descriptor.Name;

    /// <summary>The workflow input type.</summary>
    public Type InputType => Descriptor.InputType;

    /// <summary>Whether a host trigger has been configured.</summary>
    public bool HasTrigger => _trigger is not null;

    /// <summary>
    /// Reads payloads from this workflow's trigger.
    /// </summary>
    /// <param name="services">The service provider used to resolve the trigger.</param>
    /// <param name="cancellationToken">The host cancellation token.</param>
    /// <returns>The trigger payload stream.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when this workflow has no trigger.
    /// </exception>
    public IAsyncEnumerable<object?> ReadAllAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);
        return _trigger?.Invoke(services, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Workflow '{Name}' does not have a host trigger.");
    }

    internal ValueTask<object?> ExecuteAsync(
        IServiceProvider services,
        object? input,
        WorkflowContext context,
        CancellationToken cancellationToken) =>
        _pipeline(services, input, context, cancellationToken);

    internal IReadOnlyList<Type> MiddlewareTypes { get; }
}
