namespace Novolis.WorkflowEngine;

/// <summary>
/// Resolves immutable workflow definitions registered in dependency injection.
/// </summary>
public sealed class WorkflowRegistry : IWorkflowRegistry
{
    private readonly IReadOnlyDictionary<string, WorkflowDefinition> _definitions;

    /// <summary>
    /// Creates a registry from all workflow definitions in the service collection.
    /// </summary>
    /// <param name="definitions">The registered workflow definitions.</param>
    public WorkflowRegistry(IEnumerable<WorkflowDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        var entries = definitions.ToArray();
        _definitions = entries
            .GroupBy(definition => definition.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Count() switch
                {
                    1 => group.Single(),
                    _ => throw new InvalidOperationException(
                        $"Workflow name '{group.Key}' is registered more than once.")
                },
                StringComparer.OrdinalIgnoreCase);
        Definitions = entries
            .Select(definition => definition.Descriptor)
            .OrderBy(definition => definition.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyCollection<WorkflowDefinitionDescriptor> Definitions { get; }

    /// <summary>
    /// Resolves a complete workflow definition.
    /// </summary>
    /// <param name="workflowName">The registered workflow name.</param>
    /// <returns>The workflow definition.</returns>
    public WorkflowDefinition GetDefinition(string workflowName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowName);
        return _definitions.TryGetValue(workflowName, out var definition)
            ? definition
            : throw new WorkflowNotFoundException(workflowName);
    }

    /// <inheritdoc />
    public WorkflowDefinitionDescriptor Get(string workflowName) =>
        GetDefinition(workflowName).Descriptor;
}
