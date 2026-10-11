using Novolis.WorkflowEngine.Graph;

namespace Novolis.WorkflowEngine.Unit;

public sealed class GraphPackageTests
{
    [Test]
    public async Task Graph_mutations_return_new_snapshots()
    {
        var source = new NodeId(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var target = new NodeId(Guid.Parse("00000000-0000-0000-0000-000000000002"));
        var edge = new EdgeId(Guid.Parse("00000000-0000-0000-0000-000000000003"));
        var original = new Graph<string>().AddNode(source, "source");

        var changed = original
            .AddNode(target, "target")
            .Connect(edge, source, target);

        await Assert.That(original.Nodes.Count).IsEqualTo(1);
        await Assert.That(original.Edges.Count).IsEqualTo(0);
        await Assert.That(changed.Nodes.Count).IsEqualTo(2);
        await Assert.That(changed.Edges.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Topological_order_is_stable_for_fan_in_and_fan_out()
    {
        var first = new NodeId(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var second = new NodeId(Guid.Parse("00000000-0000-0000-0000-000000000002"));
        var third = new NodeId(Guid.Parse("00000000-0000-0000-0000-000000000003"));
        var fourth = new NodeId(Guid.Parse("00000000-0000-0000-0000-000000000004"));
        var graph = new Graph<string>()
            .AddNode(third, "third")
            .AddNode(first, "first")
            .AddNode(fourth, "fourth")
            .AddNode(second, "second")
            .Connect(EdgeId.New(), first, third)
            .Connect(EdgeId.New(), second, third)
            .Connect(EdgeId.New(), third, fourth);

        var analysis = new GraphAnalyzer().Analyze(graph);

        await Assert.That(analysis.IsValid).IsTrue();
        await Assert.That(analysis.TopologicalOrder.Count).IsEqualTo(4);
        await Assert.That(analysis.TopologicalOrder[0]).IsEqualTo(first);
        await Assert.That(analysis.TopologicalOrder[1]).IsEqualTo(second);
        await Assert.That(analysis.TopologicalOrder[2]).IsEqualTo(third);
        await Assert.That(analysis.TopologicalOrder[3]).IsEqualTo(fourth);
    }

    [Test]
    public async Task Cycles_are_reported_and_can_be_rejected_by_policy()
    {
        var first = new NodeId(Guid.Parse("00000000-0000-0000-0000-000000000011"));
        var second = new NodeId(Guid.Parse("00000000-0000-0000-0000-000000000012"));
        var graph = new Graph<string>()
            .AddNode(first, "first")
            .AddNode(second, "second")
            .Connect(EdgeId.New(), first, second)
            .Connect(EdgeId.New(), second, first);

        var reported = new GraphAnalyzer().Analyze(graph);
        var rejected = new GraphAnalyzer().Analyze(
            graph,
            new GraphAnalysisOptions { RejectCycles = true });

        await Assert.That(reported.IsAcyclic).IsFalse();
        await Assert.That(reported.Diagnostics.Any(diagnostic =>
            diagnostic.Code == GraphDiagnosticCode.Cycle)).IsTrue();
        await Assert.That(rejected.HasErrors).IsTrue();
    }

    [Test]
    public async Task Affected_downstream_includes_the_changed_node()
    {
        var first = NodeId.New();
        var second = NodeId.New();
        var third = NodeId.New();
        var unrelated = NodeId.New();
        var graph = new Graph<string>()
            .AddNode(first, "first")
            .AddNode(second, "second")
            .AddNode(third, "third")
            .AddNode(unrelated, "unrelated")
            .Connect(EdgeId.New(), first, second)
            .Connect(EdgeId.New(), second, third);

        var affected = new GraphAnalyzer().AffectedDownstream(graph, [first]);

        await Assert.That(affected.Contains(first)).IsTrue();
        await Assert.That(affected.Contains(second)).IsTrue();
        await Assert.That(affected.Contains(third)).IsTrue();
        await Assert.That(affected.Contains(unrelated)).IsFalse();
    }
}
