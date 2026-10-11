using System.Collections.Immutable;
using Novolis.WorkflowEngine.Graph;
using Novolis.WorkflowEngine.Graph.Export.Mermaid;
using Novolis.WorkflowEngine.Graph.Export.Svg;
using Novolis.WorkflowEngine.Graph.Layout;

namespace Novolis.WorkflowEngine.Unit;

public sealed class GraphProjectionTests
{
    [Test]
    public async Task Layered_layout_is_deterministic_and_routes_edges()
    {
        var source = new NodeId(Guid.Parse(
            "00000000-0000-0000-0000-000000000001"));
        var target = new NodeId(Guid.Parse(
            "00000000-0000-0000-0000-000000000002"));
        var graph = new Graph<string>()
            .AddNode(target, "target")
            .AddNode(source, "source")
            .Connect(
                new EdgeId(Guid.Parse(
                    "00000000-0000-0000-0000-000000000003")),
                source,
                target);

        var engine = new LayeredGraphLayout();
        var first = engine.Layout(graph);
        var second = engine.Layout(graph);

        await Assert.That(first.Nodes).IsEquivalentTo(second.Nodes);
        await Assert.That(first.Edges).IsEquivalentTo(second.Edges);
        await Assert.That(first.Bounds).IsEqualTo(second.Bounds);
        await Assert.That(first.Nodes[0].Label).IsEqualTo("source");
        await Assert.That(first.Nodes[1].Layer).IsEqualTo(1);
        await Assert.That(first.Edges.Single().Route.Count).IsEqualTo(2);
    }

    [Test]
    public async Task Pinned_positions_are_view_state_and_do_not_change_semantics()
    {
        var node = new NodeId(Guid.Parse(
            "00000000-0000-0000-0000-000000000011"));
        var graph = new Graph<string>().AddNode(node, "pinned");
        var state = new GraphViewState
        {
            PinnedPositions = ImmutableDictionary<string, GraphPoint>.Empty
                .Add(node.ToString(), new GraphPoint(240d, 80d)),
        };

        var layout = new LayeredGraphLayout().Layout(graph, viewState: state);

        await Assert.That(layout.Nodes.Single().IsPinned).IsTrue();
        await Assert.That(layout.Nodes.Single().Bounds.X).IsEqualTo(240d);
        await Assert.That(layout.Nodes.Single().Bounds.Y).IsEqualTo(80d);
        await Assert.That(graph.Nodes.Single().Value).IsEqualTo("pinned");
    }

    [Test]
    public async Task Mermaid_and_svg_escape_labels()
    {
        var node = new NodeId(Guid.Parse(
            "00000000-0000-0000-0000-000000000021"));
        var graph = new Graph<string>().AddNode(node, "Steel \"<plate>\"");
        var layout = new LayeredGraphLayout().Layout(graph);

        var mermaid = MermaidGraphExporter.Export(graph, layout);
        var svg = SvgGraphExporter.Export(layout);

        await Assert.That(mermaid).Contains("\\\"");
        await Assert.That(svg).Contains("Steel \"&lt;plate&gt;\"");
        await Assert.That(svg).Contains("<svg");
    }
}
