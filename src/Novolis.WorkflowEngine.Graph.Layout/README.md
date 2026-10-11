# Novolis.WorkflowEngine.Graph.Layout

Deterministic graph layout and editor-only view state.

The package consumes semantic graph snapshots and keeps positions, viewport,
collapse state, and route geometry outside graph equality and semantic
fingerprints. It has no editor controls or execution behavior.

```csharp
using Novolis.WorkflowEngine.Graph;
using Novolis.WorkflowEngine.Graph.Layout;

var graph = new Graph<string>()
    .AddNode(NodeId.New(), "source");
var result = new LayeredGraphLayout().Layout(graph);
```
