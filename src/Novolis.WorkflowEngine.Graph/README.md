# Novolis.WorkflowEngine.Graph

Immutable graph topology for compile-time and diagnostic workflows.

The package contains no execution engine, dependency-injection integration,
editor layout state, or rendering dependency. Graph snapshots can contain
cycles; consumers choose whether a cycle is valid for their domain.

```csharp
using Novolis.WorkflowEngine.Graph;

var source = NodeId.New();
var target = NodeId.New();
var graph = new Graph<string>()
    .AddNode(source, "source")
    .AddNode(target, "target")
    .Connect(EdgeId.New(), source, target);

var analysis = new GraphAnalyzer().Analyze(graph);
var order = analysis.TopologicalOrder;
```
