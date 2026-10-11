# Novolis.WorkflowEngine.FlowGraph

Immutable typed flow graphs for validation and compilation planning.

The package combines `Novolis.WorkflowEngine.Graph` topology with
`Novolis.WorkflowEngine.Flow` port contracts. `TryConnect` validates a
connection before returning a new snapshot. The package does not execute node
delegates, schedule jobs, or own editor layout state.

```csharp
using Novolis.WorkflowEngine.Flow;
using Novolis.WorkflowEngine.FlowGraph;

var source = new FlowNode(NodeId.New(), new FlowNodeDescriptor(
    "Source", 1,
    [new PortDescriptor(new PortId("output"), PortDirection.Output, FlowType.Scalar)]));
var target = new FlowNode(NodeId.New(), new FlowNodeDescriptor(
    "Target", 1,
    [new PortDescriptor(new PortId("input"), PortDirection.Input, FlowType.Scalar)]));

var graph = new FlowGraph()
    .AddNode(source)
    .AddNode(target);
var connection = graph.TryConnect(
    EdgeId.New(),
    new FlowPortReference(source.Id, new PortId("output")),
    new FlowPortReference(target.Id, new PortId("input")));
graph = connection.Graph;
```
