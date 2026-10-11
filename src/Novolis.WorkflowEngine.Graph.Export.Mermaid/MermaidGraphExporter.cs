using System.Text;
using Novolis.WorkflowEngine.Graph;
using Novolis.WorkflowEngine.Graph.Layout;

namespace Novolis.WorkflowEngine.Graph.Export.Mermaid;

/// <summary>
/// Writes a deterministic Mermaid flowchart from graph semantics.
/// </summary>
public static class MermaidGraphExporter
{
    /// <summary>
    /// Exports a graph as a left-to-right Mermaid flowchart.
    /// </summary>
    public static string Export<T>(
        Graph<T> graph,
        GraphLayoutResult? layout = null,
        Func<GraphNode<T>, string>? labelSelector = null)
    {
        ArgumentNullException.ThrowIfNull(graph);
        layout ??= new LayeredGraphLayout().Layout(
            graph,
            labelSelector);

        var labels = layout.Nodes.ToDictionary(
            node => node.Id,
            node => node.Label,
            StringComparer.Ordinal);
        var builder = new StringBuilder("flowchart LR");
        foreach (var node in layout.Nodes.OrderBy(node => node.Id, StringComparer.Ordinal))
        {
            builder
                .AppendLine()
                .Append("    ")
                .Append(MermaidId(node.Id))
                .Append("[\"")
                .Append(EscapeLabel(node.Label))
                .Append("\"]");
        }

        foreach (var edge in layout.Edges
                     .OrderBy(edge => edge.SourceId, StringComparer.Ordinal)
                     .ThenBy(edge => edge.TargetId, StringComparer.Ordinal))
        {
            if (!labels.ContainsKey(edge.SourceId) ||
                !labels.ContainsKey(edge.TargetId))
            {
                continue;
            }

            builder
                .AppendLine()
                .Append("    ")
                .Append(MermaidId(edge.SourceId))
                .Append(" --> ")
                .Append(MermaidId(edge.TargetId));
        }

        return builder.ToString();
    }

    private static string MermaidId(string id) =>
        "n" + id.Replace("-", string.Empty, StringComparison.Ordinal);

    private static string EscapeLabel(string label) =>
        (label ?? string.Empty)
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal);
}
