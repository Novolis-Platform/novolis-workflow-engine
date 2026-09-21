<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-workflow-engine">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
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
