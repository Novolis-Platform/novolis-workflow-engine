<!-- novolis-pkg-brand:start -->
[![Novolis](https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.png)](https://novolis-platform.github.io/.github/novolis-workflow-engine/)

[Novolis](https://github.com/Novolis-Platform) · [Docs](https://novolis-platform.github.io/.github/novolis-workflow-engine/) · [Source](https://github.com/Novolis-Platform/novolis-workflow-engine)
<!-- novolis-pkg-brand:end -->

# Novolis.WorkflowEngine.Mapping

Adds `ThenMap<TMapping, TInput, TOutput>()` for workflows that already use a
`Novolis.Mapping` definition.

Register the mapping implementation with DI, then compose it as a normal
workflow transformation:

```csharp
services.AddScoped<RawToNormalizedMapping>();
services.AddWorkflow("normalize", workflow => workflow
    .Accepts<RawMessage>()
    .ThenMap<RawToNormalizedMapping, RawMessage, NormalizedMessage>());
```
