using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;

namespace WebExample.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IProducer<Null, WeatherForecast> _producer;

    public WeatherForecastController(ILogger<WeatherForecastController> logger,
        IProducer<Null, WeatherForecast> producer)
    {
        _logger = logger;
        _producer = producer;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
            .ToArray();
    }

    [HttpPost(Name = "PostWeatherForecast")]
    public async Task<IActionResult> Post(WeatherForecast forecast, CancellationToken cancellationToken)
    {
        await _producer.ProduceAsync("forecasts", new Message<Null, WeatherForecast> { Value = forecast },
            cancellationToken);

        return Ok();
    }
}
