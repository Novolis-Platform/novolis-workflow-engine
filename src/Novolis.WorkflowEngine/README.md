<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-workflow-engine">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.WorkflowEngine

Host-independent workflow definitions and execution.

```csharp
services.AddWorkflow("normalize", workflow => workflow
    .Accepts<RawMessage>()
    .Then<NormalizeStep, RawMessage, NormalizedMessage>()
    .EndWith<StoreSink, NormalizedMessage>());

var result = await services
    .BuildServiceProvider()
    .GetRequiredService<IWorkflowEngine>()
    .ExecuteAsync("normalize", new RawMessage("hello"));

result.ThrowIfFailed();
```

The core package does not own channels, background services, cron schedules, or
application lifetime. Add `Novolis.WorkflowEngine.Hosting` for triggers and
`Novolis.WorkflowEngine.Channels` or `Novolis.WorkflowEngine.Scheduling` for
specific input sources.
