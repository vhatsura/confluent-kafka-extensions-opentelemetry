using System.Diagnostics;
using System.Text;
using Confluent.Kafka;

namespace WebExample;

public static class ConsumerExtensions
{
    public static async Task ConsumeWithInstrumentation<TKey, TValue>(this IConsumer<TKey, TValue> consumer,
        Func<ConsumeResult<TKey, TValue>, CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        var consumeResult = consumer.Consume(cancellationToken);

        var activity = new ActivitySource("Confluent.Kafka.Extensions.Diagnostics").CreateActivity(
            "Confluent.Kafka.Consume",
            ActivityKind.Consumer,
            default(ActivityContext),
            new[]
            {
                new KeyValuePair<string, object>("messaging.system", "kafka"),
                new KeyValuePair<string, object>("messaging.destination", consumeResult.Topic),
                new KeyValuePair<string, object>("messaging.destination_kind", "topic"),
                new KeyValuePair<string, object>("messaging.kafka.partition",
                    consumeResult.TopicPartition.Partition.ToString())
            }!);

        if (activity != null)
        {
            var traceParentHeader = consumeResult.Message.Headers?.FirstOrDefault(x => x.Key == "traceparent");
            var traceStateHeader = consumeResult.Message.Headers?.FirstOrDefault(x => x.Key == "tracestate");

            var traceParent = traceParentHeader != null
                ? Encoding.UTF8.GetString(traceParentHeader.GetValueBytes())
                : null;
            var traceState = traceStateHeader != null
                ? Encoding.UTF8.GetString(traceStateHeader.GetValueBytes())
                : null;

            if (ActivityContext.TryParse(traceParent, traceState, out var activityContext))
            {
                activity.SetParentId(activityContext.TraceId, activityContext.SpanId, activityContext.TraceFlags);
                activity.TraceStateString = activityContext.TraceState;
            }
        }

        activity?.Start();

        await action(consumeResult, cancellationToken);

        activity?.Stop();
    }
}
