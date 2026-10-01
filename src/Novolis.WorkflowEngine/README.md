<!-- novolis-pkg-brand:start -->
[![Novolis](https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.png)](https://novolis-platform.github.io/.github/novolis-workflow-engine/)

[Novolis](https://github.com/Novolis-Platform) · [Docs](https://novolis-platform.github.io/.github/novolis-workflow-engine/) · [Source](https://github.com/Novolis-Platform/novolis-workflow-engine)
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
