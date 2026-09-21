<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-workflow-engine">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.WorkflowEngine.Channels

Use a `System.Threading.Channels` stream as a workflow trigger without
coupling the engine core to channel registration.

```csharp
services.AddWorkflowChannelTrigger<RawMessage>();
services.AddWorkflow("normalize", workflow => workflow
    .TriggeredBy<ChannelWorkflowTrigger<RawMessage>, RawMessage>()
    .Then<NormalizeStep, RawMessage, NormalizedMessage>()
    .EndWith<StoreSink, NormalizedMessage>());
```
