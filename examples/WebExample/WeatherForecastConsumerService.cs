using Confluent.Kafka;

namespace WebExample;

public class WeatherForecastConsumerService : BackgroundService
{
    private readonly IConsumer<Ignore, WeatherForecast> _consumer;
    private readonly HttpClient _client;

    public WeatherForecastConsumerService(IConsumer<Ignore, WeatherForecast> consumer, IHttpClientFactory clientFactory)
    {
        _consumer = consumer;
        _client = clientFactory.CreateClient();
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _consumer.Subscribe("forecasts");

        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            await _consumer.ConsumeWithInstrumentation(async (result, cancellationToken) =>
            {
                await _client.PostAsJsonAsync("https://httpbin.org/post", result.Message.Value,
                    cancellationToken);
            }, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        _consumer.Close();
    }

    public override void Dispose()
    {
        base.Dispose();
        _consumer.Dispose();
    }
}
