# Confluent.Kafka.Extensions.OpenTelemetry

![GitHub Actions Badge](https://github.com/vhatsura/confluent-kafka-extensions-opentelemetry/actions/workflows/continuous.integration.yml/badge.svg)
[![NuGet Badge](https://buildstats.info/nuget/Confluent.Kafka.Extensions.OpenTelemetry)](https://www.nuget.org/packages/Confluent.Kafka.Extensions.OpenTelemetry/)

The `Confluent.Kafka.Extensions.OpenTelemetry` package enables collection of instrumentation data of the `Confluent.Kafka` library.
The actual instrumentation of the `Confluent.Kafka` library should be configured using
[Confluent.Kafka.Extensions.Diagnostics](https://github.com/vhatsura/confluent-kafka-extensions-diagnostics).

## Installation

```powershell
Install-Package Confluent.Kafka.Extensions.OpenTelemetry
```

## Usage

### Confluent.Kafka configuration

As `Confluent.Kafka` does not expose any instrumentation data, additional, configuration is required.
Full documentation is available at [Confluent.Kafka.Extensions.Diagnostics docs](https://github.com/vhatsura/confluent-kafka-extensions-diagnostics#usage).
There is also an [example](./examples/WebExample) on how to use the package in real world application.

#### Producer

```csharp
using Confluent.Kafka;
using Confluent.Kafka.Extensions.Diagnostics;


using var producer =
    new ProducerBuilder<Null, string>(new ProducerConfig(new ClientConfig { BootstrapServers = "localhost:9092" }))
        .SetKeySerializer(Serializers.Null)
        .SetValueSerializer(Serializers.Utf8)
        .BuildWithInstrumentation();

await producer.ProduceAsync("topic", new Message<Null, string> { Value = "Hello World!" });
```

#### Consumer

```csharp
using Confluent.Kafka;
using Confluent.Kafka.Extensions.Diagnostics;

using var consumer = new ConsumerBuilder<Ignore, string>(
        new ConsumerConfig(new ClientConfig { BootstrapServers = "localhost:9092" })
        {
            GroupId = "group", AutoOffsetReset = AutoOffsetReset.Earliest
        })
    .SetValueDeserializer(Deserializers.Utf8)
    .Build();

consumer.Subscribe("topic");

consumer.ConsumeWithInstrumentation((result) =>
{
    Console.WriteLine(result.Message.Value);
});
```

### OpenTelemetry configuration

```csharp
using Confluent.Kafka.Extensions.OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

builder.Services.AddOpenTelemetry().WithTracing(traceBuilder =>
{
    traceBuilder
        .AddInMemoryExporter()
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddConfluentKafkaInstrumentation();  // <-- Add Confluent.Kafka OpenTelemetry support
});
```
