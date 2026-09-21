<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-workflow-engine">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.WorkflowEngine

Typed, channel-backed workflow steps for the .NET generic host.

## Install

```powershell
dotnet add package Novolis.WorkflowEngine
```

## Compose a workflow

```csharp
builder.Services.AddWorkflow(workflow =>
{
    workflow
        .StartWith<ReadStep, Input>()
        .Then<TransformStep, Input, Output>()
        .ThenEndWith<WriteStep, Output>();
});
```

`StartWith` runs its `IStartStep<T>` when the host starts. Each `Then` consumes
one input and publishes one output. `ThenEndWith` consumes the final value.
Channels are registered once per payload type, so adjacent steps may share a
payload type.

See the repository README for the complete example.
