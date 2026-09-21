<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-workflow-engine">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
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
