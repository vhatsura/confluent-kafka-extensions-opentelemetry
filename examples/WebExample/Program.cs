using Confluent.Kafka;
using Confluent.Kafka.Extensions.Diagnostics;
using Confluent.Kafka.Extensions.OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WebExample;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

var kafkaServers = "localhost:9092";

builder.Services.AddSingleton(_ =>
{
    return new ProducerBuilder<Null, WeatherForecast>(
            new ProducerConfig(new ClientConfig { BootstrapServers = kafkaServers }))
        .SetKeySerializer(Serializers.Null)
        .SetValueSerializer(new KafkaJsonSerializer<WeatherForecast>())
        .BuildWithInstrumentation();
});

builder.Services.AddSingleton(_ =>
{
    return new ConsumerBuilder<Ignore, WeatherForecast>(
            new ConsumerConfig(new ClientConfig { BootstrapServers = kafkaServers })
            {
                GroupId = "group1",
                AutoOffsetReset = AutoOffsetReset.Earliest
            })
        .SetValueDeserializer(new KafkaJsonSerializer<WeatherForecast>())
        .Build();
});

builder.Services.AddHostedService<WeatherForecastConsumerService>();

builder.Services.AddOpenTelemetry().WithTracing(traceBuilder =>
{
    traceBuilder.AddSource("webExample")
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService("webExample", serviceVersion: "1.0.0")
            .AddAttributes(new[]
            {
                new KeyValuePair<string, object>("deployment.environment",
                    builder.Environment.EnvironmentName)
            }))
        .AddZipkinExporter(o =>
        {
            o.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
        })
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddConfluentKafkaInstrumentation();
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
