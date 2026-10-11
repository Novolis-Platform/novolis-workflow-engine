using System.Collections.Frozen;
using System.Collections.ObjectModel;
using Novolis.WorkflowEngine.Flow;
using Novolis.WorkflowEngine.Graph;

namespace Novolis.WorkflowEngine.FlowGraph;

/// <summary>
/// An immutable graph combining topology with typed flow ports.
/// </summary>
public sealed class FlowGraph
{
    private readonly ReadOnlyCollection<FlowNode> _nodes;
    private readonly ReadOnlyCollection<FlowConnection> _connections;
    private readonly ReadOnlyCollection<FlowSubgraphReference> _subgraphs;

    /// <summary>
    /// Creates a flow graph snapshot.
    /// </summary>
    /// <param name="nodes">The node instances.</param>
    /// <param name="connections">The port connections.</param>
    /// <param name="subgraphs">The referenced subgraphs.</param>
    public FlowGraph(
        IEnumerable<FlowNode>? nodes = null,
        IEnumerable<FlowConnection>? connections = null,
        IEnumerable<FlowSubgraphReference>? subgraphs = null)
    {
        _nodes = new ReadOnlyCollection<FlowNode>(
            (nodes ?? []).OrderBy(node => node.Id).ToArray());
        _connections = new ReadOnlyCollection<FlowConnection>(
            (connections ?? []).OrderBy(connection => connection.Id).ToArray());
        _subgraphs = new ReadOnlyCollection<FlowSubgraphReference>(
            (subgraphs ?? []).OrderBy(subgraph => subgraph.Id, StringComparer.Ordinal).ToArray());
    }

    /// <summary>Gets an empty flow graph.</summary>
    public static FlowGraph Empty { get; } = new();

    /// <summary>Gets the node instances in stable identity order.</summary>
    public IReadOnlyList<FlowNode> Nodes => _nodes;

    /// <summary>Gets the connections in stable identity order.</summary>
    public IReadOnlyList<FlowConnection> Connections => _connections;

    /// <summary>Gets the referenced subgraphs in stable identity order.</summary>
    public IReadOnlyList<FlowSubgraphReference> Subgraphs => _subgraphs;

    /// <summary>
    /// Gets the topology projection used by graph analyzers.
    /// </summary>
    public Graph<FlowNode> Topology =>
        new(
            _nodes.Select(node => new GraphNode<FlowNode>(node.Id, node)),
            _connections.Select(connection => new GraphEdge(
                connection.Id,
                connection.Source.NodeId,
                connection.Destination.NodeId,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["sourcePort"] = connection.Source.PortId.Value,
                    ["destinationPort"] = connection.Destination.PortId.Value,
                })));

    /// <summary>
    /// Adds a node and returns a new snapshot.
    /// </summary>
    public FlowGraph AddNode(FlowNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (_nodes.Any(candidate => candidate.Id == node.Id))
        {
            throw new ArgumentException(
                $"A node with identity '{node.Id}' already exists.",
                nameof(node));
        }

        return new FlowGraph(_nodes.Append(node), _connections, _subgraphs);
    }

    /// <summary>
    /// Removes a node and all connections attached to it.
    /// </summary>
    public FlowGraph RemoveNode(NodeId nodeId)
    {
        if (_nodes.All(node => node.Id != nodeId))
        {
            return this;
        }

        return new FlowGraph(
            _nodes.Where(node => node.Id != nodeId),
            _connections.Where(connection =>
                connection.Source.NodeId != nodeId &&
                connection.Destination.NodeId != nodeId),
            _subgraphs);
    }

    /// <summary>
    /// Adds a subgraph reference and returns a new snapshot.
    /// </summary>
    public FlowGraph AddSubgraphReference(FlowSubgraphReference subgraph)
    {
        ArgumentNullException.ThrowIfNull(subgraph);
        if (_subgraphs.Any(candidate =>
                string.Equals(candidate.Id, subgraph.Id, StringComparison.Ordinal)))
        {
            throw new ArgumentException(
                $"A subgraph with identity '{subgraph.Id}' already exists.",
                nameof(subgraph));
        }

        return new FlowGraph(_nodes, _connections, _subgraphs.Append(subgraph));
    }

    /// <summary>
    /// Removes a subgraph reference and returns a new snapshot.
    /// </summary>
    public FlowGraph RemoveSubgraphReference(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return new FlowGraph(
            _nodes,
            _connections,
            _subgraphs.Where(subgraph =>
                !string.Equals(subgraph.Id, id, StringComparison.Ordinal)));
    }

    /// <summary>
    /// Attempts to connect an output port to an input port.
    /// </summary>
    public FlowGraphConnectionResult TryConnect(
        EdgeId edgeId,
        FlowPortReference source,
        FlowPortReference destination)
    {
        var diagnostics = new List<FlowGraphDiagnostic>();
        if (_connections.Any(connection => connection.Id == edgeId))
        {
            diagnostics.Add(new FlowGraphDiagnostic(
                FlowGraphDiagnosticCode.DuplicateConnectionId,
                FlowGraphDiagnosticSeverity.Error,
                $"A connection with identity '{edgeId}' already exists.",
                EdgeId: edgeId));
        }

        var sourceNode = _nodes.FirstOrDefault(node => node.Id == source.NodeId);
        var destinationNode = _nodes.FirstOrDefault(node => node.Id == destination.NodeId);
        PortDescriptor? sourcePort = null;
        PortDescriptor? destinationPort = null;

        if (sourceNode is null)
        {
            diagnostics.Add(new FlowGraphDiagnostic(
                FlowGraphDiagnosticCode.MissingSourceNode,
                FlowGraphDiagnosticSeverity.Error,
                $"Connection '{edgeId}' refers to missing source node '{source.NodeId}'.",
                NodeId: source.NodeId,
                EdgeId: edgeId,
                SourcePort: source.PortId,
                DestinationPort: destination.PortId));
        }
        else if (!sourceNode.Descriptor.TryGetPort(source.PortId, out sourcePort))
        {
            diagnostics.Add(new FlowGraphDiagnostic(
                FlowGraphDiagnosticCode.MissingSourcePort,
                FlowGraphDiagnosticSeverity.Error,
                $"Node '{source.NodeId}' has no source port '{source.PortId}'.",
                NodeId: source.NodeId,
                EdgeId: edgeId,
                SourcePort: source.PortId,
                DestinationPort: destination.PortId));
        }

        if (destinationNode is null)
        {
            diagnostics.Add(new FlowGraphDiagnostic(
                FlowGraphDiagnosticCode.MissingDestinationNode,
                FlowGraphDiagnosticSeverity.Error,
                $"Connection '{edgeId}' refers to missing destination node '{destination.NodeId}'.",
                NodeId: destination.NodeId,
                EdgeId: edgeId,
                SourcePort: source.PortId,
                DestinationPort: destination.PortId));
        }
        else if (!destinationNode.Descriptor.TryGetPort(
                     destination.PortId,
                     out destinationPort))
        {
            diagnostics.Add(new FlowGraphDiagnostic(
                FlowGraphDiagnosticCode.MissingDestinationPort,
                FlowGraphDiagnosticSeverity.Error,
                $"Node '{destination.NodeId}' has no destination port '{destination.PortId}'.",
                NodeId: destination.NodeId,
                EdgeId: edgeId,
                SourcePort: source.PortId,
                DestinationPort: destination.PortId));
        }

        if (sourcePort is not null && destinationPort is not null)
        {
            var existingSourceConnections = _connections.Count(connection =>
                connection.Source == source);
            var existingDestinationConnections = _connections.Count(connection =>
                connection.Destination == destination);
            var contract = FlowConnectionValidator.Default.Validate(
                sourcePort,
                destinationPort,
                existingDestinationConnections,
                existingSourceConnections);
            diagnostics.AddRange(contract.Diagnostics.Select(diagnostic =>
                ToGraphDiagnostic(diagnostic, edgeId, source.NodeId)));
        }

        if (diagnostics.Count > 0)
        {
            return new FlowGraphConnectionResult(false, this, SortDiagnostics(diagnostics));
        }

        return new FlowGraphConnectionResult(
            true,
            new FlowGraph(
                _nodes,
                _connections.Append(new FlowConnection(edgeId, source, destination)),
                _subgraphs),
            []);
    }

    /// <summary>
    /// Attempts to connect two ports using a new stable edge identity.
    /// </summary>
    public FlowGraphConnectionResult TryConnect(
        FlowPortReference source,
        FlowPortReference destination) =>
        TryConnect(EdgeId.New(), source, destination);

    /// <summary>
    /// Attempts to connect two ports and returns the resulting snapshot.
    /// </summary>
    public bool TryConnect(
        EdgeId edgeId,
        FlowPortReference source,
        FlowPortReference destination,
        out FlowGraph graph,
        out IReadOnlyList<FlowGraphDiagnostic> diagnostics)
    {
        var result = TryConnect(edgeId, source, destination);
        graph = result.Graph;
        diagnostics = result.Diagnostics;
        return result.IsValid;
    }

    /// <summary>
    /// Connects two ports or throws when the connection is invalid.
    /// </summary>
    public FlowGraph Connect(
        EdgeId edgeId,
        FlowPortReference source,
        FlowPortReference destination)
    {
        var result = TryConnect(edgeId, source, destination);
        if (!result.IsValid)
        {
            throw new InvalidOperationException(
                string.Join(
                    Environment.NewLine,
                    result.Diagnostics.Select(diagnostic => diagnostic.Message)));
        }

        return result.Graph;
    }

    /// <summary>
    /// Removes a connection and returns a new snapshot.
    /// </summary>
    public FlowGraph RemoveConnection(EdgeId edgeId)
    {
        if (_connections.All(connection => connection.Id != edgeId))
        {
            return this;
        }

        return new FlowGraph(
            _nodes,
            _connections.Where(connection => connection.Id != edgeId),
            _subgraphs);
    }

    /// <summary>
    /// Validates graph topology, ports, cardinality, and subgraph references.
    /// </summary>
    public FlowGraphValidationResult Validate(
        FlowGraphValidationOptions? options = null)
    {
        options ??= FlowGraphValidationOptions.DagOnly;
        var topology = GraphAnalyzer.Default.Analyze(
            Topology,
            new GraphAnalysisOptions
            {
                RejectCycles = options.RejectCycles,
                RejectSelfLoops = options.RejectCycles,
            });
        var diagnostics = topology.Diagnostics
            .Select(ToGraphDiagnostic)
            .ToList();

        foreach (var group in _connections
                     .GroupBy(connection => connection.Id)
                     .Where(group => group.Count() > 1))
        {
            diagnostics.Add(new FlowGraphDiagnostic(
                FlowGraphDiagnosticCode.DuplicateConnectionId,
                FlowGraphDiagnosticSeverity.Error,
                $"Connection identity '{group.Key}' occurs {group.Count()} times.",
                EdgeId: group.Key));
        }

        foreach (var connection in _connections.OrderBy(candidate => candidate.Id))
        {
            ValidateConnection(connection, diagnostics, options);
        }

        if (options.RequireAllInputsConnected)
        {
            ValidateRequiredInputs(diagnostics);
        }

        ValidateSubgraphs(diagnostics, options);
        diagnostics.Sort(CompareDiagnostics);
        return new FlowGraphValidationResult(topology, diagnostics);
    }

    /// <summary>
    /// Gets all nodes reachable from the specified node identities.
    /// </summary>
    public IReadOnlySet<NodeId> ReachableFrom(IEnumerable<NodeId> roots)
    {
        ArgumentNullException.ThrowIfNull(roots);
        var known = _nodes.Select(node => node.Id).ToFrozenSet();
        var adjacency = BuildAdjacency();
        var result = new HashSet<NodeId>();
        var pending = new Queue<NodeId>();
        foreach (var root in roots.Where(known.Contains).OrderBy(nodeId => nodeId))
        {
            if (result.Add(root))
            {
                pending.Enqueue(root);
            }
        }

        while (pending.Count > 0)
        {
            var current = pending.Dequeue();
            foreach (var next in adjacency[current])
            {
                if (result.Add(next))
                {
                    pending.Enqueue(next);
                }
            }
        }

        return result.ToFrozenSet();
    }

    /// <summary>
    /// Gets all nodes reachable from one root identity.
    /// </summary>
    public IReadOnlySet<NodeId> ReachableFrom(NodeId root) =>
        ReachableFrom([root]);

    /// <summary>
    /// Gets terminal output nodes affected by changed node identities.
    /// </summary>
    public IReadOnlySet<NodeId> GetAffectedOutputs(
        IEnumerable<NodeId> changedNodeIds)
    {
        var affected = ReachableFrom(changedNodeIds);
        var adjacency = BuildAdjacency();
        return affected
            .Where(nodeId => adjacency[nodeId].Count == 0)
            .ToFrozenSet();
    }

    /// <summary>
    /// Gets selected output nodes affected by changed node identities.
    /// </summary>
    public IReadOnlySet<NodeId> GetAffectedOutputs(
        IEnumerable<NodeId> changedNodeIds,
        IEnumerable<NodeId> outputNodeIds)
    {
        ArgumentNullException.ThrowIfNull(outputNodeIds);
        var affected = ReachableFrom(changedNodeIds);
        return outputNodeIds
            .Where(affected.Contains)
            .ToFrozenSet();
    }

    /// <summary>
    /// Gets a deterministic topological node order under the DAG policy.
    /// </summary>
    public IReadOnlyList<NodeId> TopologicalSort()
    {
        var validation = Validate(FlowGraphValidationOptions.DagOnly);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(
                string.Join(
                    Environment.NewLine,
                    validation.Diagnostics.Select(diagnostic => diagnostic.Message)));
        }

        return validation.Topology.TopologicalOrder;
    }

    private IReadOnlyDictionary<NodeId, SortedSet<NodeId>> BuildAdjacency()
    {
        var adjacency = _nodes.ToDictionary(
            node => node.Id,
            _ => new SortedSet<NodeId>());
        foreach (var connection in _connections)
        {
            if (adjacency.TryGetValue(connection.Source.NodeId, out var destinations) &&
                adjacency.ContainsKey(connection.Destination.NodeId))
            {
                destinations.Add(connection.Destination.NodeId);
            }
        }

        return adjacency;
    }

    private void ValidateConnection(
        FlowConnection connection,
        ICollection<FlowGraphDiagnostic> diagnostics,
        FlowGraphValidationOptions options)
    {
        var sourceNode = _nodes.FirstOrDefault(node =>
            node.Id == connection.Source.NodeId);
        var destinationNode = _nodes.FirstOrDefault(node =>
            node.Id == connection.Destination.NodeId);
        if (sourceNode is null || destinationNode is null)
        {
            return;
        }

        if (!sourceNode.Descriptor.TryGetPort(
                connection.Source.PortId,
                out var sourcePort) ||
            !destinationNode.Descriptor.TryGetPort(
                connection.Destination.PortId,
                out var destinationPort) ||
            sourcePort is null ||
            destinationPort is null)
        {
            return;
        }

        var sourceConnections = _connections.Count(candidate =>
            candidate.Source == connection.Source);
        var destinationConnections = _connections.Count(candidate =>
            candidate.Destination == connection.Destination);
        var contract = FlowConnectionValidator.Default.Validate(
            sourcePort,
            destinationPort,
            destinationConnections - 1,
            sourceConnections - 1);
        foreach (var diagnostic in contract.Diagnostics)
        {
            diagnostics.Add(ToGraphDiagnostic(
                diagnostic,
                connection.Id,
                connection.Source.NodeId));
        }

    }

    private void ValidateRequiredInputs(
        ICollection<FlowGraphDiagnostic> diagnostics)
    {
        foreach (var node in _nodes.OrderBy(candidate => candidate.Id))
        {
            foreach (var input in node.Descriptor.InputPorts.Where(
                         port => port.Cardinality == PortCardinality.Single &&
                                 !_connections.Any(candidate =>
                                     candidate.Destination.NodeId == node.Id &&
                                     candidate.Destination.PortId == port.Id)))
            {
                diagnostics.Add(new FlowGraphDiagnostic(
                    FlowGraphDiagnosticCode.RequiredInputUnconnected,
                    FlowGraphDiagnosticSeverity.Error,
                    $"Required input '{input.Id}' on node '{node.Id}' is not connected.",
                    NodeId: node.Id,
                    DestinationPort: input.Id));
            }
        }
    }

    private void ValidateSubgraphs(
        ICollection<FlowGraphDiagnostic> diagnostics,
        FlowGraphValidationOptions options)
    {
        var groups = _subgraphs
            .GroupBy(subgraph => subgraph.Id, StringComparer.Ordinal)
            .ToArray();
        foreach (var group in groups.Where(group => group.Count() > 1))
        {
            diagnostics.Add(new FlowGraphDiagnostic(
                FlowGraphDiagnosticCode.DuplicateSubgraphId,
                FlowGraphDiagnosticSeverity.Error,
                $"Subgraph identity '{group.Key}' occurs {group.Count()} times."));
        }

        var known = groups.ToDictionary(
            group => group.Key,
            group => group.First(),
            StringComparer.Ordinal);
        foreach (var subgraph in _subgraphs)
        {
            foreach (var dependency in subgraph.Dependencies)
            {
                if (!known.ContainsKey(dependency))
                {
                    diagnostics.Add(new FlowGraphDiagnostic(
                        FlowGraphDiagnosticCode.MissingSubgraphDependency,
                        FlowGraphDiagnosticSeverity.Error,
                        $"Subgraph '{subgraph.Id}' refers to missing dependency '{dependency}'."));
                }
            }
        }

        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);

        void Visit(string id, IReadOnlyList<string> path)
        {
            if (visiting.Contains(id))
            {
                if (options.RejectRecursiveSubgraphs)
                {
                    diagnostics.Add(new FlowGraphDiagnostic(
                        FlowGraphDiagnosticCode.RecursiveSubgraph,
                        FlowGraphDiagnosticSeverity.Error,
                        $"Recursive subgraph dependency detected: {string.Join(" -> ", path.Append(id))}."));
                }

                return;
            }

            if (!visited.Add(id) || !known.TryGetValue(id, out var current))
            {
                return;
            }

            visiting.Add(id);
            foreach (var dependency in current.Dependencies.Order(StringComparer.Ordinal))
            {
                Visit(dependency, path.Append(id).ToArray());
            }

            visiting.Remove(id);
        }

        foreach (var subgraph in _subgraphs.OrderBy(
                     candidate => candidate.Id,
                     StringComparer.Ordinal))
        {
            Visit(subgraph.Id, []);
        }
    }

    private static FlowGraphDiagnostic ToGraphDiagnostic(
        GraphDiagnostic diagnostic) =>
        new(
            diagnostic.Code switch
            {
                GraphDiagnosticCode.DuplicateNodeId => FlowGraphDiagnosticCode.DuplicateNodeId,
                GraphDiagnosticCode.DuplicateEdgeId => FlowGraphDiagnosticCode.DuplicateConnectionId,
                GraphDiagnosticCode.MissingSourceNode => FlowGraphDiagnosticCode.MissingSourceNode,
                GraphDiagnosticCode.MissingDestinationNode => FlowGraphDiagnosticCode.MissingDestinationNode,
                GraphDiagnosticCode.DuplicateEdgeEndpoints =>
                    FlowGraphDiagnosticCode.DuplicateConnectionEndpoints,
                GraphDiagnosticCode.SelfLoop => FlowGraphDiagnosticCode.SelfLoop,
                GraphDiagnosticCode.Cycle => FlowGraphDiagnosticCode.Cycle,
                GraphDiagnosticCode.InaccessibleNode => FlowGraphDiagnosticCode.InaccessibleNode,
                _ => throw new ArgumentOutOfRangeException(),
            },
            (FlowGraphDiagnosticSeverity)diagnostic.Severity,
            diagnostic.Message,
            diagnostic.NodeId,
            diagnostic.EdgeId);

    private static FlowGraphDiagnostic ToGraphDiagnostic(
        FlowDiagnostic diagnostic,
        EdgeId edgeId,
        NodeId nodeId) =>
        new(
            diagnostic.Code switch
            {
                FlowDiagnosticCode.SourceIsNotOutput =>
                    FlowGraphDiagnosticCode.SourceIsNotOutput,
                FlowDiagnosticCode.DestinationIsNotInput =>
                    FlowGraphDiagnosticCode.DestinationIsNotInput,
                FlowDiagnosticCode.TypeMismatch => FlowGraphDiagnosticCode.TypeMismatch,
                FlowDiagnosticCode.CardinalityExceeded =>
                    FlowGraphDiagnosticCode.CardinalityExceeded,
                FlowDiagnosticCode.InvalidPort => FlowGraphDiagnosticCode.MissingSourcePort,
                _ => throw new ArgumentOutOfRangeException(),
            },
            (FlowGraphDiagnosticSeverity)diagnostic.Severity,
            diagnostic.Message,
            nodeId,
            edgeId,
            diagnostic.SourcePort,
            diagnostic.DestinationPort);

    private static IReadOnlyList<FlowGraphDiagnostic> SortDiagnostics(
        IEnumerable<FlowGraphDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(diagnostic => diagnostic.Code)
            .ThenBy(diagnostic => diagnostic.NodeId)
            .ThenBy(diagnostic => diagnostic.EdgeId)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.Ordinal)
            .ToArray();

    private static int CompareDiagnostics(
        FlowGraphDiagnostic left,
        FlowGraphDiagnostic right)
    {
        var codeComparison = left.Code.CompareTo(right.Code);
        if (codeComparison != 0)
        {
            return codeComparison;
        }

        var nodeComparison = Nullable.Compare(left.NodeId, right.NodeId);
        if (nodeComparison != 0)
        {
            return nodeComparison;
        }

        var edgeComparison = Nullable.Compare(left.EdgeId, right.EdgeId);
        if (edgeComparison != 0)
        {
            return edgeComparison;
        }

        return string.Compare(left.Message, right.Message, StringComparison.Ordinal);
    }
}
