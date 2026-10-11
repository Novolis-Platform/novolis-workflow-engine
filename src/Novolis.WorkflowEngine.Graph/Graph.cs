using System.Collections.ObjectModel;

namespace Novolis.WorkflowEngine.Graph;

/// <summary>
/// An immutable snapshot of directed graph topology and caller-owned node values.
/// </summary>
/// <typeparam name="T">The node value type.</typeparam>
public sealed class Graph<T> : IEquatable<Graph<T>>
{
    private readonly ReadOnlyCollection<GraphNode<T>> _nodes;
    private readonly ReadOnlyCollection<GraphEdge> _edges;

    /// <summary>
    /// Creates a graph snapshot.
    /// </summary>
    /// <param name="nodes">The nodes in the snapshot.</param>
    /// <param name="edges">The directed edges in the snapshot.</param>
    public Graph(
        IEnumerable<GraphNode<T>>? nodes = null,
        IEnumerable<GraphEdge>? edges = null)
    {
        _nodes = Array.AsReadOnly(
            (nodes ?? []).OrderBy(node => node.Id).ToArray());
        _edges = Array.AsReadOnly(
            (edges ?? []).OrderBy(edge => edge.Id).ToArray());
    }

    /// <summary>
    /// Gets an empty graph.
    /// </summary>
    public static Graph<T> Empty { get; } = new([], []);

    /// <summary>
    /// Gets the nodes in stable identity order.
    /// </summary>
    public IReadOnlyList<GraphNode<T>> Nodes => _nodes;

    /// <summary>
    /// Gets the edges in stable identity order.
    /// </summary>
    public IReadOnlyList<GraphEdge> Edges => _edges;

    /// <summary>
    /// Determines whether a node identity is present.
    /// </summary>
    public bool ContainsNode(NodeId id) => _nodes.Any(node => node.Id == id);

    /// <summary>
    /// Determines whether an edge identity is present.
    /// </summary>
    public bool ContainsEdge(EdgeId id) => _edges.Any(edge => edge.Id == id);

    /// <summary>
    /// Tries to get a node by identity.
    /// </summary>
    public bool TryGetNode(NodeId id, out GraphNode<T>? node)
    {
        node = _nodes.FirstOrDefault(candidate => candidate.Id == id);
        return node is not null;
    }

    /// <summary>
    /// Adds a node and returns a new graph snapshot.
    /// </summary>
    public Graph<T> AddNode(NodeId id, T value) => AddNode(new GraphNode<T>(id, value));

    /// <summary>
    /// Adds a node and returns a new graph snapshot.
    /// </summary>
    public Graph<T> AddNode(GraphNode<T> node)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (ContainsNode(node.Id))
        {
            throw new ArgumentException(
                $"A node with identity '{node.Id}' already exists.",
                nameof(node));
        }

        return new Graph<T>(_nodes.Append(node), _edges);
    }

    /// <summary>
    /// Removes a node and its incident edges, returning a new graph snapshot.
    /// </summary>
    public Graph<T> RemoveNode(NodeId id)
    {
        if (!ContainsNode(id))
        {
            return this;
        }

        return new Graph<T>(
            _nodes.Where(node => node.Id != id),
            _edges.Where(edge => edge.From != id && edge.To != id));
    }

    /// <summary>
    /// Adds an edge and returns a new graph snapshot.
    /// </summary>
    public Graph<T> AddEdge(GraphEdge edge)
    {
        ArgumentNullException.ThrowIfNull(edge);
        if (ContainsEdge(edge.Id))
        {
            throw new ArgumentException(
                $"An edge with identity '{edge.Id}' already exists.",
                nameof(edge));
        }

        if (!ContainsNode(edge.From))
        {
            throw new ArgumentException(
                $"The edge source '{edge.From}' is not present in the graph.",
                nameof(edge));
        }

        if (!ContainsNode(edge.To))
        {
            throw new ArgumentException(
                $"The edge destination '{edge.To}' is not present in the graph.",
                nameof(edge));
        }

        return new Graph<T>(_nodes, _edges.Append(edge));
    }

    /// <summary>
    /// Adds an edge and returns a new graph snapshot.
    /// </summary>
    public Graph<T> Connect(
        EdgeId id,
        NodeId from,
        NodeId to,
        IReadOnlyDictionary<string, string>? metadata = null) =>
        AddEdge(new GraphEdge(id, from, to, metadata));

    /// <summary>
    /// Removes an edge and returns a new graph snapshot.
    /// </summary>
    public Graph<T> RemoveEdge(EdgeId id)
    {
        if (!ContainsEdge(id))
        {
            return this;
        }

        return new Graph<T>(_nodes, _edges.Where(edge => edge.Id != id));
    }

    /// <summary>
    /// Gets the node identities directly downstream from a node.
    /// </summary>
    public IReadOnlyList<NodeId> Downstream(NodeId id) =>
        _edges
            .Where(edge => edge.From == id)
            .Select(edge => edge.To)
            .Distinct()
            .OrderBy(nodeId => nodeId)
            .ToArray();

    /// <summary>
    /// Gets the node identities directly upstream from a node.
    /// </summary>
    public IReadOnlyList<NodeId> Upstream(NodeId id) =>
        _edges
            .Where(edge => edge.To == id)
            .Select(edge => edge.From)
            .Distinct()
            .OrderBy(nodeId => nodeId)
            .ToArray();

    /// <summary>
    /// Compares graph topology and node values.
    /// </summary>
    public bool Equals(Graph<T>? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null ||
            _nodes.Count != other._nodes.Count ||
            _edges.Count != other._edges.Count)
        {
            return false;
        }

        return _nodes.SequenceEqual(other._nodes) &&
               _edges.SequenceEqual(other._edges);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Graph<T> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var node in _nodes)
        {
            hash.Add(node);
        }

        foreach (var edge in _edges)
        {
            hash.Add(edge);
        }

        return hash.ToHashCode();
    }
}
