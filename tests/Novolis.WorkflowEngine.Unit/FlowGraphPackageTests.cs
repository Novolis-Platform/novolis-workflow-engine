using Novolis.WorkflowEngine.Flow;
using Novolis.WorkflowEngine.FlowGraph;
using Novolis.WorkflowEngine.Graph;
using TypedFlowGraph = Novolis.WorkflowEngine.FlowGraph.FlowGraph;

namespace Novolis.WorkflowEngine.Unit;

public sealed class FlowGraphPackageTests
{
    [Test]
    public async Task Valid_connection_returns_new_snapshot()
    {
        var source = CreateSource();
        var target = CreateTarget();
        var original = new TypedFlowGraph()
            .AddNode(source)
            .AddNode(target);

        var result = original.TryConnect(
            new EdgeId(Guid.Parse("00000000-0000-0000-0000-000000000101")),
            new FlowPortReference(source.Id, new PortId("output")),
            new FlowPortReference(target.Id, new PortId("input")));

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(original.Connections.Count).IsEqualTo(0);
        await Assert.That(result.Graph.Connections.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Invalid_connection_does_not_mutate_the_graph()
    {
        var source = CreateSource();
        var target = CreateTarget(FlowType.Vector3);
        var original = new TypedFlowGraph()
            .AddNode(source)
            .AddNode(target);

        var result = original.TryConnect(
            EdgeId.New(),
            new FlowPortReference(source.Id, new PortId("output")),
            new FlowPortReference(target.Id, new PortId("input")));

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Graph).IsSameReferenceAs(original);
        await Assert.That(result.Diagnostics.Any(diagnostic =>
            diagnostic.Code == FlowGraphDiagnosticCode.TypeMismatch)).IsTrue();
    }

    [Test]
    public async Task Dag_validation_and_affected_outputs_follow_port_connections()
    {
        var source = CreateSource();
        var middle = CreateTarget();
        var target = CreateTarget();
        var graph = new TypedFlowGraph()
            .AddNode(source)
            .AddNode(middle)
            .AddNode(target)
            .Connect(
                EdgeId.New(),
                new FlowPortReference(source.Id, new PortId("output")),
                new FlowPortReference(middle.Id, new PortId("input")))
            .Connect(
                EdgeId.New(),
                new FlowPortReference(middle.Id, new PortId("output")),
                new FlowPortReference(target.Id, new PortId("input")));

        var validation = graph.Validate();
        var affectedOutputs = graph.GetAffectedOutputs([source.Id]);

        await Assert.That(validation.IsValid).IsTrue();
        await Assert.That(graph.TopologicalSort().Count).IsEqualTo(3);
        await Assert.That(affectedOutputs.Contains(target.Id)).IsTrue();
        await Assert.That(affectedOutputs.Contains(source.Id)).IsFalse();
    }

    [Test]
    public async Task Recursive_subgraphs_are_rejected_by_default()
    {
        var graph = new TypedFlowGraph(
            subgraphs:
            [
                new FlowSubgraphReference(
                    "a",
                    1,
                    "hash-a",
                    dependencies: ["b"]),
                new FlowSubgraphReference(
                    "b",
                    1,
                    "hash-b",
                    dependencies: ["a"]),
            ]);

        var validation = graph.Validate();

        await Assert.That(validation.IsValid).IsFalse();
        await Assert.That(validation.Diagnostics.Any(diagnostic =>
            diagnostic.Code == FlowGraphDiagnosticCode.RecursiveSubgraph)).IsTrue();
    }

    private static FlowNode CreateSource() =>
        new(
            NodeId.New(),
            new FlowNodeDescriptor(
                "Source",
                1,
                [
                    new PortDescriptor(
                        new PortId("output"),
                        PortDirection.Output,
                        FlowType.Scalar),
                ]));

    private static FlowNode CreateTarget(FlowType? inputType = null) =>
        new(
            NodeId.New(),
            new FlowNodeDescriptor(
                "Target",
                1,
                [
                    new PortDescriptor(
                        new PortId("input"),
                        PortDirection.Input,
                        inputType ?? FlowType.Scalar),
                    new PortDescriptor(
                        new PortId("output"),
                        PortDirection.Output,
                        FlowType.Scalar),
                ]));
}
