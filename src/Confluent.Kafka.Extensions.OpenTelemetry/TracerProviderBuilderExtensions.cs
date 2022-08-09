using OpenTelemetry.Trace;

namespace Confluent.Kafka.Extensions.OpenTelemetry;

public static class TracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddConfluentKafkaInstrumentation(this TracerProviderBuilder builder)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        builder.AddSource("Confluent.Kafka.Extensions.Diagnostics");

        return builder;
    }
}
