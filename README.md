<!-- novolis-marketing:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-brand-transparent.svg" width="360" alt="Novolis"/>
  </a>
</p>

<p align="center">
  <strong>Typed channel workflows for .NET hosts</strong><br/>
  Compose start, transform, and end steps with dependency injection and hosted services.
</p>
<!-- novolis-marketing:end -->

# novolis-workflow-engine

`Novolis.WorkflowEngine` is a small, typed workflow pipeline for the .NET generic host.
Each step is connected by a `System.Threading.Channels` channel registered through
`Microsoft.Extensions.DependencyInjection`.

The package is the Novolis extraction of `Frank.WorkflowEngine`. The workflow engine
uses `Novolis.Messaging.Channels`, with `Novolis.Mapping` and `Novolis.Scheduling`
available as companion infrastructure packages for workflow hosts.

## Install

```powershell
dotnet add package Novolis.WorkflowEngine
```

## Quick start

```csharp
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Novolis.WorkflowEngine;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWorkflow(workflow =>
{
    workflow
        .StartWith<ReadStep, Input>()
        .Then<TransformStep, Input, Output>()
        .ThenEndWith<WriteStep, Output>();
});

await builder.Build().RunAsync();

sealed record Input(string Value);
sealed record Output(string Value);

sealed class ReadStep(ChannelWriter<Input> writer) : IStartStep<Input>
{
    public Task RunAsync(CancellationToken cancellationToken) =>
        writer.WriteAsync(new Input("hello"), cancellationToken).AsTask();
}

sealed class TransformStep : IStep<Input, Output>
{
    public Task<Output> ExecuteAsync(Input input) =>
        Task.FromResult(new Output(input.Value.ToUpperInvariant()));
}

sealed class WriteStep : IEndStep<Output>
{
    public Task ExecuteAsync(Output result)
    {
        Console.WriteLine(result.Value);
        return Task.CompletedTask;
    }
}
```

`StartWith` registers a hosted start runner, so the start step runs when the host
starts. Intermediate and end steps run continuously until the host is stopped.

## Related projects

- `src/Novolis.WorkflowEngine` — packable library.
- `tests/Novolis.WorkflowEngine.Unit` — TUnit coverage for registration and execution.
- `d:\novolis\novolis-lab\labs\workflows\WorkflowEngineLab` — runnable integration sample.

## Support

Pre-release packages use the `2026.1.*` version line on GitHub Packages.
