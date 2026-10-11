using System.Collections.ObjectModel;

namespace Novolis.WorkflowEngine.Flow;

/// <summary>
/// Result of checking one proposed flow connection.
/// </summary>
public sealed class FlowConnectionResult
{
    internal FlowConnectionResult(
        bool isValid,
        IReadOnlyList<FlowDiagnostic> diagnostics)
    {
        IsValid = isValid;
        Diagnostics = new ReadOnlyCollection<FlowDiagnostic>(diagnostics.ToArray());
    }

    /// <summary>Gets a value indicating whether the connection is valid.</summary>
    public bool IsValid { get; }

    /// <summary>Gets diagnostics explaining the result.</summary>
    public IReadOnlyList<FlowDiagnostic> Diagnostics { get; }
}
