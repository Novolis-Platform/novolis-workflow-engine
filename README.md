<!-- novolis-marketing:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-brand-transparent.svg" width="360" alt="Novolis"/>
  </a>
</p>

<p align="center">
  <strong>Composable workflows for .NET applications</strong><br/>
  Define named pipelines once, invoke them manually, or attach an input source.
</p>
<!-- novolis-marketing:end -->

# novolis-workflow-engine

`Novolis.WorkflowEngine` is the Novolis extraction of `Frank.WorkflowEngine`,
redesigned around named definitions, typed transformations, per-run scopes,
explicit results, and composable middleware.

The engine core is host-independent. Channels, cron, and generic-host pumps are
separate packages so an application can use only the integration it needs.

## Install

```powershell
dotnet add package Novolis.WorkflowEngine
```

## Quick start: manual execution

```csharp
using Microsoft.Extensions.DependencyInjection;
using Novolis.WorkflowEngine;

var services = new ServiceCollection();
services.AddWorkflow("normalize", workflow => workflow
    .Accepts<RawMessage>()
    .Use<TraceMiddleware>()
    .Then<NormalizeStep, RawMessage, NormalizedMessage>()
    .EndWith<StoreSink, NormalizedMessage>());

var engine = services
    .BuildServiceProvider()
    .GetRequiredService<IWorkflowEngine>();

var result = await engine.ExecuteAsync(
    "normalize",
    new RawMessage("hello"));

result.ThrowIfFailed();
```

## Quick start: hosted channel input

```csharp
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Novolis.WorkflowEngine;
using Novolis.WorkflowEngine.Channels;
using Novolis.WorkflowEngine.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWorkflowChannelTrigger<RawMessage>();
builder.Services.AddWorkflow("normalize", workflow =>
    workflow
        .TriggeredBy<ChannelWorkflowTrigger<RawMessage>, RawMessage>()
        .Then<NormalizeStep, RawMessage, NormalizedMessage>()
        .EndWith<StoreSink, NormalizedMessage>());
builder.Services.AddWorkflowHosting();

using var host = builder.Build();
await host.StartAsync();
await host.Services
    .GetRequiredService<ChannelWriter<RawMessage>>()
    .WriteAsync(new RawMessage("hello"));
await host.WaitForShutdownAsync();
```

Implement `IWorkflowTrigger<T>` for message buses, file watchers, timers, or
application-specific sources. Add `Novolis.WorkflowEngine.Scheduling` for
`CronWorkflowTrigger<T>`, or `Novolis.WorkflowEngine.Mapping` for
`ThenMap<TMapping, TInput, TOutput>()`.

## Related projects

- `src/Novolis.WorkflowEngine.Abstractions` — stable contracts and execution records.
- `src/Novolis.WorkflowEngine` — host-independent registry and engine.
- `src/Novolis.WorkflowEngine.Hosting` — generic-host trigger pump.
- `src/Novolis.WorkflowEngine.Channels` — `System.Threading.Channels` adapter.
- `src/Novolis.WorkflowEngine.Mapping` — `Novolis.Mapping` adapter.
- `src/Novolis.WorkflowEngine.Scheduling` — `Novolis.Scheduling` cron adapter.
- `tests/Novolis.WorkflowEngine.Unit` — TUnit coverage for all components.
- `d:\novolis\novolis-lab\labs\workflows\WorkflowEngineLab` — runnable integration sample.

## Tests

The repository uses TUnit's executable test runner:

```powershell
dotnet run --project tests/Novolis.WorkflowEngine.Unit/Novolis.WorkflowEngine.Unit.csproj
```

## Support

Pre-release packages use the `2026.1.*` version line on GitHub Packages.
