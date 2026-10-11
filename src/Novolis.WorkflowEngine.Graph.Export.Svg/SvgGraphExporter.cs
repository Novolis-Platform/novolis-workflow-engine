using System.Globalization;
using System.Xml.Linq;
using Novolis.WorkflowEngine.Graph.Layout;

namespace Novolis.WorkflowEngine.Graph.Export.Svg;

/// <summary>
/// Writes a graph layout as a standalone SVG document.
/// </summary>
public static class SvgGraphExporter
{
    /// <summary>
    /// Exports a deterministic SVG document.
    /// </summary>
    public static string Export(
        GraphLayoutResult layout,
        SvgGraphExportOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(layout);
        options ??= new SvgGraphExportOptions();
        options.Validate();

        var padding = options.Padding;
        var width = Math.Max(
            options.MinimumWidth,
            layout.Bounds.Width + padding * 2d);
        var height = Math.Max(
            options.MinimumHeight,
            layout.Bounds.Height + padding * 2d);
        var offsetX = padding - layout.Bounds.X;
        var offsetY = padding - layout.Bounds.Y;
        XNamespace ns = "http://www.w3.org/2000/svg";

        var svg = new XElement(
            ns + "svg",
            new XAttribute("width", Format(width)),
            new XAttribute("height", Format(height)),
            new XAttribute("viewBox", $"0 0 {Format(width)} {Format(height)}"),
            new XElement(
                ns + "defs",
                new XElement(
                    ns + "marker",
                    new XAttribute("id", "arrow"),
                    new XAttribute("markerWidth", "10"),
                    new XAttribute("markerHeight", "7"),
                    new XAttribute("refX", "9"),
                    new XAttribute("refY", "3.5"),
                    new XAttribute("orient", "auto"),
                    new XElement(
                        ns + "path",
                        new XAttribute("d", "M0,0 L10,3.5 L0,7 z"),
                        new XAttribute("fill", options.EdgeColor)))),
            new XElement(
                ns + "g",
                new XAttribute("class", "edges"),
                layout.Edges.Select(edge => CreateEdge(
                    ns,
                    edge,
                    offsetX,
                    offsetY,
                    options))),
            new XElement(
                ns + "g",
                new XAttribute("class", "nodes"),
                layout.Nodes.Select(node => CreateNode(
                    ns,
                    node,
                    offsetX,
                    offsetY,
                    options))));

        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            svg).ToString(SaveOptions.DisableFormatting);
    }

    private static XElement CreateEdge(
        XNamespace ns,
        GraphLayoutEdge edge,
        double offsetX,
        double offsetY,
        SvgGraphExportOptions options)
    {
        var points = string.Join(
            " ",
            edge.Route.Select(point =>
                $"{Format(point.X + offsetX)},{Format(point.Y + offsetY)}"));
        return new XElement(
            ns + "polyline",
            new XAttribute("class", "edge"),
            new XAttribute("data-source", edge.SourceId),
            new XAttribute("data-target", edge.TargetId),
            new XAttribute("points", points),
            new XAttribute("fill", "none"),
            new XAttribute("stroke", options.EdgeColor),
            new XAttribute("stroke-width", Format(options.EdgeWidth)),
            new XAttribute("marker-end", "url(#arrow)"));
    }

    private static XElement CreateNode(
        XNamespace ns,
        GraphLayoutNode node,
        double offsetX,
        double offsetY,
        SvgGraphExportOptions options)
    {
        var bounds = node.Bounds;
        var x = bounds.X + offsetX;
        var y = bounds.Y + offsetY;
        return new XElement(
            ns + "g",
            new XAttribute("id", "node-" + node.Id),
            new XAttribute("class", "node"),
            new XAttribute("data-node-id", node.Id),
            new XElement(
                ns + "rect",
                new XAttribute("x", Format(x)),
                new XAttribute("y", Format(y)),
                new XAttribute("width", Format(bounds.Width)),
                new XAttribute("height", Format(bounds.Height)),
                new XAttribute("rx", Format(options.CornerRadius)),
                new XAttribute("fill", options.NodeFill),
                new XAttribute("stroke", options.NodeStroke),
                new XAttribute("stroke-width", Format(options.NodeStrokeWidth))),
            new XElement(
                ns + "text",
                new XAttribute("x", Format(bounds.CenterX + offsetX)),
                new XAttribute("y", Format(bounds.CenterY + offsetY)),
                new XAttribute("text-anchor", "middle"),
                new XAttribute("dominant-baseline", "middle"),
                new XAttribute("fill", options.TextColor),
                node.Label));
    }

    private static string Format(double value) =>
        value.ToString("0.###", CultureInfo.InvariantCulture);
}
