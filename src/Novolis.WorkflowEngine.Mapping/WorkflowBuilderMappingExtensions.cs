namespace Novolis.WorkflowEngine.Mapping;

/// <summary>
/// Fluent mapping adapters for workflow definitions.
/// </summary>
public static class WorkflowBuilderMappingExtensions
{
    /// <summary>
    /// Adds a <see cref="Novolis.Mapping.IMappingDefinition{TSource, TDestination}"/>
    /// as a workflow transformation.
    /// </summary>
    /// <typeparam name="TMapping">The mapping implementation.</typeparam>
    /// <typeparam name="TInput">The source payload type.</typeparam>
    /// <typeparam name="TOutput">The destination payload type.</typeparam>
    /// <param name="builder">The workflow builder.</param>
    /// <returns>The same builder.</returns>
    public static WorkflowBuilder ThenMap<TMapping, TInput, TOutput>(
        this WorkflowBuilder builder)
        where TMapping : class, Novolis.Mapping.IMappingDefinition<TInput, TOutput>
        where TInput : class
        where TOutput : class =>
        builder.Then<MappingWorkflowStep<TMapping, TInput, TOutput>, TInput, TOutput>();
}
