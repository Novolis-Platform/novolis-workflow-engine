# Novolis.WorkflowEngine.Flow

Semantic flow contracts without graph storage or execution.

`FlowType` describes logical values independently of CLR assemblies. Port
descriptors describe direction and cardinality, while `FlowConnectionValidator`
checks a proposed output-to-input connection and returns diagnostics.

```csharp
using Novolis.WorkflowEngine.Flow;

var output = new PortDescriptor(
    new PortId("value"),
    PortDirection.Output,
    FlowType.Scalar);
var input = new PortDescriptor(
    new PortId("value"),
    PortDirection.Input,
    FlowType.Scalar);

var result = FlowConnectionValidator.Default.Validate(output, input);
```
