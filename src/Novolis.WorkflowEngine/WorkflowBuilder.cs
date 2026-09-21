using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Novolis.WorkflowEngine;

/// <summary>
/// Fluent definition builder for one named workflow.
/// </summary>
public sealed class WorkflowBuilder
{
    private readonly IServiceCollection _services;
    private readonly string _name;
    private readonly List<Type> _middlewareTypes = [];
    private WorkflowDelegate _pipeline = static (input, _, _) =>
        ValueTask.FromResult(input);
    private Func<IServiceProvider, CancellationToken, IAsyncEnumerable<object?>>? _trigger;
    private Type? _triggerType;
    private Type? _inputType;
    private Type? _currentType;
    private bool _hasTerminalSink;

    internal WorkflowBuilder(IServiceCollection services, string name)
    {
        _services = services;
        _name = name;
    }

    /// <summary>
    /// Declares the input type for a manually invoked workflow.
    /// </summary>
    /// <typeparam name="TInput">The workflow input type.</typeparam>
    /// <returns>This builder.</returns>
    public WorkflowBuilder Accepts<TInput>()
        where TInput : class
    {
        EnsureInputNotDeclared();
        _inputType = typeof(TInput);
        _currentType = typeof(TInput);
        return this;
    }

    /// <summary>
    /// Connects a trigger to the workflow.
    /// </summary>
    /// <typeparam name="TTrigger">The trigger implementation.</typeparam>
    /// <typeparam name="TInput">The trigger payload type.</typeparam>
    /// <returns>This builder.</returns>
    public WorkflowBuilder TriggeredBy<TTrigger, TInput>()
        where TTrigger : class, IWorkflowTrigger<TInput>
        where TInput : class
    {
        EnsureInputNotDeclared();
        _inputType = typeof(TInput);
        _currentType = typeof(TInput);
        _triggerType = typeof(TTrigger);
        _trigger = ReadTriggerAsync<TTrigger, TInput>;
        _services.TryAddScoped<TTrigger>();
        return this;
    }

    /// <summary>
    /// Adds a typed transformation step.
    /// </summary>
    /// <typeparam name="TStep">The step implementation.</typeparam>
    /// <typeparam name="TInput">The step input type.</typeparam>
    /// <typeparam name="TOutput">The step output type.</typeparam>
    /// <returns>This builder.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the step input does not match the preceding output type.
    /// </exception>
    public WorkflowBuilder Then<TStep, TInput, TOutput>()
        where TStep : class, IWorkflowStep<TInput, TOutput>
        where TInput : class
        where TOutput : class
    {
        EnsureCanAppend(typeof(TInput));
        var previous = _pipeline;
        _pipeline = (input, context, cancellationToken) =>
            InvokeStepAsync<TStep, TInput, TOutput>(
                previous,
                input,
                context,
                cancellationToken);
        _services.TryAddScoped<TStep>();
        _currentType = typeof(TOutput);
        return this;
    }

    /// <summary>
    /// Adds the terminal sink for a workflow.
    /// </summary>
    /// <typeparam name="TSink">The sink implementation.</typeparam>
    /// <typeparam name="TPayload">The sink input type.</typeparam>
    /// <returns>This builder.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the sink input does not match the preceding output type or a
    /// sink has already been configured.
    /// </exception>
    public WorkflowBuilder EndWith<TSink, TPayload>()
        where TSink : class, IWorkflowSink<TPayload>
        where TPayload : class
    {
        if (_hasTerminalSink)
        {
            throw new InvalidOperationException(
                $"Workflow '{_name}' already has a terminal sink.");
        }

        EnsureCanAppend(typeof(TPayload));
        var previous = _pipeline;
        _pipeline = (input, context, cancellationToken) =>
            InvokeSinkAsync<TSink, TPayload>(
                previous,
                input,
                context,
                cancellationToken);
        _services.TryAddScoped<TSink>();
        _hasTerminalSink = true;
        return this;
    }

    /// <summary>
    /// Adds cross-cutting behavior around every execution of this workflow.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware implementation.</typeparam>
    /// <returns>This builder.</returns>
    public WorkflowBuilder Use<TMiddleware>()
        where TMiddleware : class, IWorkflowMiddleware
    {
        _services.TryAddScoped<TMiddleware>();
        _middlewareTypes.Add(typeof(TMiddleware));
        return this;
    }

    /// <summary>
    /// Builds the immutable workflow definition.
    /// </summary>
    /// <returns>The workflow definition.</returns>
    public WorkflowDefinition Build()
    {
        if (_inputType is null || _currentType is null)
        {
            throw new InvalidOperationException(
                $"Workflow '{_name}' must declare input with Accepts<T>() or TriggeredBy<TTrigger, T>().");
        }

        var descriptor = new WorkflowDefinitionDescriptor(
            _name,
            _inputType,
            _triggerType,
            _currentType,
            _hasTerminalSink);

        return new WorkflowDefinition(
            descriptor,
            _pipeline,
            _trigger,
            _middlewareTypes.ToArray());
    }

    private void EnsureInputNotDeclared()
    {
        if (_inputType is not null)
        {
            throw new InvalidOperationException(
                $"Workflow '{_name}' already declares input type '{_inputType.FullName}'.");
        }
    }

    private void EnsureCanAppend(Type expectedInputType)
    {
        if (_inputType is null || _currentType is null)
        {
            throw new InvalidOperationException(
                $"Workflow '{_name}' must declare input before adding steps.");
        }

        if (_hasTerminalSink)
        {
            throw new InvalidOperationException(
                $"Workflow '{_name}' cannot add a step after its terminal sink.");
        }

        if (_currentType != expectedInputType)
        {
            throw new InvalidOperationException(
                $"Workflow '{_name}' expected a step accepting '{_currentType.FullName}', " +
                $"but received one accepting '{expectedInputType.FullName}'.");
        }
    }

    private static async ValueTask<object?> InvokeStepAsync<TStep, TInput, TOutput>(
        WorkflowDelegate previous,
        object? input,
        WorkflowContext context,
        CancellationToken cancellationToken)
        where TStep : class, IWorkflowStep<TInput, TOutput>
        where TInput : class
        where TOutput : class
    {
        var value = await previous(input, context, cancellationToken).ConfigureAwait(false);
        if (value is not TInput typedInput)
        {
            throw new WorkflowContractException(context.WorkflowName, typeof(TInput), value);
        }

        var step = context.Services.GetRequiredService<TStep>();
        return await step.ExecuteAsync(typedInput, context, cancellationToken)
            .ConfigureAwait(false);
    }

    private static async ValueTask<object?> InvokeSinkAsync<TSink, TPayload>(
        WorkflowDelegate previous,
        object? input,
        WorkflowContext context,
        CancellationToken cancellationToken)
        where TSink : class, IWorkflowSink<TPayload>
        where TPayload : class
    {
        var value = await previous(input, context, cancellationToken).ConfigureAwait(false);
        if (value is not TPayload typedPayload)
        {
            throw new WorkflowContractException(context.WorkflowName, typeof(TPayload), value);
        }

        var sink = context.Services.GetRequiredService<TSink>();
        await sink.HandleAsync(typedPayload, context, cancellationToken)
            .ConfigureAwait(false);
        return null;
    }

    private static async IAsyncEnumerable<object?> ReadTriggerAsync<TTrigger, TInput>(
        IServiceProvider services,
        [EnumeratorCancellation] CancellationToken cancellationToken)
        where TTrigger : class, IWorkflowTrigger<TInput>
        where TInput : class
    {
        var trigger = services.GetRequiredService<TTrigger>();
        await foreach (var payload in trigger.ReadAllAsync(cancellationToken)
                           .WithCancellation(cancellationToken)
                           .ConfigureAwait(false))
        {
            yield return payload;
        }
    }
}
