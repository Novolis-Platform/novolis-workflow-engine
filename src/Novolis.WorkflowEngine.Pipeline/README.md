# Novolis.WorkflowEngine.Pipeline

Typed linear composition for pure or application-owned transformations.

The pipeline package is independent from graph topology. It supports
`ValueTask` stages and explicit `PipelineResult<T>` stages for short-circuiting.
Retries, persistence, compensation, and host triggers remain outside this
package.

```csharp
using PipelineFactory = Novolis.WorkflowEngine.Pipeline.Pipeline;

var pipeline = PipelineFactory
    .Start<string>()
    .Then(value => value.Trim())
    .Then(value => value.ToUpperInvariant());

var result = await pipeline.ExecuteAsync(" hello ");
```
