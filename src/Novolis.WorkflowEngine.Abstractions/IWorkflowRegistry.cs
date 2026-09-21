namespace Novolis.WorkflowEngine;

/// <summary>
/// Provides metadata for workflows registered with dependency injection.
/// </summary>
public interface IWorkflowRegistry
{
    /// <summary>All registered workflow definitions.</summary>
    IReadOnlyCollection<WorkflowDefinitionDescriptor> Definitions { get; }

    /// <summary>
    /// Gets metadata for one registered workflow.
    /// </summary>
    /// <param name="workflowName">The registered workflow name.</param>
    /// <returns>The workflow metadata.</returns>
    WorkflowDefinitionDescriptor Get(string workflowName);
}
