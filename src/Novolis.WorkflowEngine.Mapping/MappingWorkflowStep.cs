using Novolis.Mapping;

namespace Novolis.WorkflowEngine.Mapping;

/// <summary>
/// Adapts a synchronous Novolis mapping definition to a workflow step.
/// </summary>
/// <typeparam name="TMapping">The mapping implementation.</typeparam>
/// <typeparam name="TInput">The source payload type.</typeparam>
/// <typeparam name="TOutput">The destination payload type.</typeparam>
public sealed class MappingWorkflowStep<TMapping, TInput, TOutput>(
    TMapping mapping) : IWorkflowStep<TInput, TOutput>
    where TMapping : class, IMappingDefinition<TInput, TOutput>
    where TInput : class
    where TOutput : class
{
    /// <inheritdoc />
    public ValueTask<TOutput> ExecuteAsync(
        TInput input,
        WorkflowContext context,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(mapping.Map(input));
}
