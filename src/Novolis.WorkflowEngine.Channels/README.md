<!-- novolis-pkg-brand:start -->
[![Novolis](https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.png)](https://novolis-platform.github.io/.github/novolis-workflow-engine/)

[Novolis](https://github.com/Novolis-Platform) · [Docs](https://novolis-platform.github.io/.github/novolis-workflow-engine/) · [Source](https://github.com/Novolis-Platform/novolis-workflow-engine)
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
