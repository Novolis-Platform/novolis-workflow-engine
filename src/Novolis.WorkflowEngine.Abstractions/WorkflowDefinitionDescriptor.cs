namespace Novolis.WorkflowEngine;

/// <summary>
/// Public metadata describing a registered workflow.
/// </summary>
public sealed record WorkflowDefinitionDescriptor(
    string Name,
    Type InputType,
    Type? TriggerType,
    Type? OutputType,
    bool HasTerminalSink);
