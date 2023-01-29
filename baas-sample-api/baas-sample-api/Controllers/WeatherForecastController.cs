using Microsoft.AspNetCore.Mvc;

namespace baas_sample_api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [ActionName("items")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }


    [HttpPost]
    [ActionName("items")]
    public WeatherForecast Post([FromBody] WeatherForecast weatherForecast)
    {
        Thread.Sleep(10000);
        return weatherForecast;
    }

    [HttpGet]
    [ActionName("items/{id}")]
    public string GetTest([FromRoute] string id)
    {


        return id;
    }


    [HttpGet]
    [ActionName("quota_with_request_header")]
    public string QuotaWithRequestHeader()
    {
        return "ok";
    }

    [HttpGet]
    [ActionName("quota_with_route/{id}")]
    public string QuotaWithRequestHeader([FromRoute] string id)
    {
        return id;
    }


    [HttpGet]
    [ActionName("quota_with_query_string")]
    public string QuotaWithQueryString([FromQuery] string name)
    {
        return name;
    }


    [HttpPost]
    [ActionName("quota_with_request_body_json_path")]
    public WeatherForecast QuotaWithRequestBodyJsonPath([FromBody] WeatherForecast weatherForecast)
    {
        return weatherForecast;
    }



}
