using OpenTelemetry.Trace;

namespace Confluent.Kafka.Extensions.OpenTelemetry;

/// <summary>
///     Extension methods for <see cref="TracerProviderBuilder" />.
/// </summary>
public static class TracerProviderBuilderExtensions
{
    /// <summary>
    ///     Enables automatic data collection for the Confluent.Kafka client.
    /// </summary>
    public static TracerProviderBuilder AddConfluentKafkaInstrumentation(this TracerProviderBuilder builder)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        builder.AddSource("Confluent.Kafka.Extensions.Diagnostics");

        return builder;
    }
}
