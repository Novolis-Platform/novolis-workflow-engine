namespace Novolis.WorkflowEngine.Flow;

/// <summary>
/// Validates typed output-to-input connections without mutating a graph.
/// </summary>
public sealed class FlowConnectionValidator
{
    /// <summary>Shared stateless validator.</summary>
    public static FlowConnectionValidator Default { get; } = new();

    /// <summary>
    /// Validates a proposed connection.
    /// </summary>
    /// <param name="source">The proposed source port.</param>
    /// <param name="destination">The proposed destination port.</param>
    /// <param name="existingDestinationConnections">
    /// Number of connections already attached to the destination.
    /// </param>
    public FlowConnectionResult Validate(
        PortDescriptor source,
        PortDescriptor destination,
        int existingDestinationConnections = 0)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        if (existingDestinationConnections < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(existingDestinationConnections));
        }

        var diagnostics = new List<FlowDiagnostic>();
        if (!source.IsOutput)
        {
            diagnostics.Add(new FlowDiagnostic(
                FlowDiagnosticCode.SourceIsNotOutput,
                FlowDiagnosticSeverity.Error,
                $"Source port '{source.Id}' must be an output.",
                SourcePort: source.Id,
                DestinationPort: destination.Id));
        }

        if (!destination.IsInput)
        {
            diagnostics.Add(new FlowDiagnostic(
                FlowDiagnosticCode.DestinationIsNotInput,
                FlowDiagnosticSeverity.Error,
                $"Destination port '{destination.Id}' must be an input.",
                SourcePort: source.Id,
                DestinationPort: destination.Id));
        }

        if (!destination.Type.IsCompatibleWith(source.Type))
        {
            diagnostics.Add(new FlowDiagnostic(
                FlowDiagnosticCode.TypeMismatch,
                FlowDiagnosticSeverity.Error,
                $"Type '{source.Type}' cannot connect to '{destination.Type}'.",
                SourcePort: source.Id,
                DestinationPort: destination.Id));
        }

        if (!destination.AllowsConnection(existingDestinationConnections))
        {
            diagnostics.Add(new FlowDiagnostic(
                FlowDiagnosticCode.CardinalityExceeded,
                FlowDiagnosticSeverity.Error,
                $"Destination port '{destination.Id}' does not accept another connection.",
                SourcePort: source.Id,
                DestinationPort: destination.Id));
        }

        return new FlowConnectionResult(diagnostics.Count == 0, diagnostics);
    }
}
