<!-- novolis-pkg-brand:start -->
[![Novolis](https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.png)](https://novolis-platform.github.io/.github/novolis-workflow-engine/)

[Novolis](https://github.com/Novolis-Platform) · [Docs](https://novolis-platform.github.io/.github/novolis-workflow-engine/) · [Source](https://github.com/Novolis-Platform/novolis-workflow-engine)
<!-- novolis-pkg-brand:end -->

# Novolis.WorkflowEngine.Scheduling

Adapts `Novolis.Scheduling` cron expressions into workflow triggers. Register
the payload factory and then use `CronWorkflowTrigger<TPayload>` in a workflow
definition.

```csharp
services.AddCronWorkflowTrigger(
    "0 */5 * * * *",
    () => new RefreshCatalog());
```
