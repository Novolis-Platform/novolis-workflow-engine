namespace Novolis.WorkflowEngine.Flow;

/// <summary>
/// Number of connections a port can accept.
/// </summary>
public enum PortCardinality
{
    /// <summary>The connection is optional and accepts at most one value.</summary>
    Optional,

    /// <summary>The connection is required and accepts at most one value.</summary>
    Single,

    /// <summary>The port accepts zero or more values.</summary>
    Many,
}
