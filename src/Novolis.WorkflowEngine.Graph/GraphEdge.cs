using System.Collections.ObjectModel;

namespace Novolis.WorkflowEngine.Graph;

/// <summary>
/// A directed graph edge.
/// </summary>
public sealed record GraphEdge
{
    /// <summary>
    /// Creates a graph edge.
    /// </summary>
    /// <param name="id">The stable edge identity.</param>
    /// <param name="from">The source node identity.</param>
    /// <param name="to">The destination node identity.</param>
    /// <param name="metadata">Optional structural metadata.</param>
    public GraphEdge(
        EdgeId id,
        NodeId from,
        NodeId to,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        if (id == EdgeId.Empty)
        {
            throw new ArgumentException("A graph edge must have a non-empty identity.", nameof(id));
        }

        if (from == NodeId.Empty)
        {
            throw new ArgumentException("An edge source must have a non-empty identity.", nameof(from));
        }

        if (to == NodeId.Empty)
        {
            throw new ArgumentException("An edge destination must have a non-empty identity.", nameof(to));
        }

        Id = id;
        From = from;
        To = to;
        Metadata = new ReadOnlyDictionary<string, string>(
            metadata is null
                ? new Dictionary<string, string>(StringComparer.Ordinal)
                : new Dictionary<string, string>(metadata, StringComparer.Ordinal));
    }

    /// <summary>The stable edge identity.</summary>
    public EdgeId Id { get; }

    /// <summary>The source node identity.</summary>
    public NodeId From { get; }

    /// <summary>The destination node identity.</summary>
    public NodeId To { get; }

    /// <summary>Structural metadata carried by this edge.</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
