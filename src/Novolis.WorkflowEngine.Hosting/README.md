<!-- novolis-pkg-brand:start -->
[![Novolis](https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.png)](https://novolis-platform.github.io/.github/novolis-workflow-engine/)

[Novolis](https://github.com/Novolis-Platform) · [Docs](https://novolis-platform.github.io/.github/novolis-workflow-engine/) · [Source](https://github.com/Novolis-Platform/novolis-workflow-engine)
<!-- novolis-pkg-brand:end -->

# Novolis.WorkflowEngine.Hosting

Runs workflow triggers as generic-host background services. Install this
package only for workflows that consume an ongoing source. Manual workflows
need only `Novolis.WorkflowEngine`.

```csharp
services.AddWorkflow("normalize", workflow => workflow
    .TriggeredBy<MyTrigger, RawMessage>()
    .Then<NormalizeStep, RawMessage, NormalizedMessage>()
    .EndWith<StoreSink, NormalizedMessage>());

services.AddWorkflowHosting();
```

Each trigger stream is hosted independently, and each payload receives a
fresh dependency-injection scope when the engine executes it.
