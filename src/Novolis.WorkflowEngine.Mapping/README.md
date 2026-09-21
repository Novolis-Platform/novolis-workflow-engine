<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-workflow-engine">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
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
