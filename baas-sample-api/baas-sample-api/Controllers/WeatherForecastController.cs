using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

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
    private readonly IConnectionMultiplexer connectionMultiplexer;
    private readonly IDatabase cache;


    public WeatherForecastController(ILogger<WeatherForecastController> logger, IConnectionMultiplexer connectionMultiplexer)
    {
        _logger = logger;
        this.connectionMultiplexer = connectionMultiplexer;
        this.cache = this.connectionMultiplexer.GetDatabase();
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
    [ActionName("redis")]
    public object RedisTest()
    {
        long transactionTime = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
        RedisResult result = this.cache.Execute("EVAL", "local data = redis.call('hsetnx',KEYS[1],KEYS[2],ARGV[1]); if data == 1 then redis.call('expire',KEYS[1],ARGV[2]); end;  redis.call('hincrby',KEYS[1],KEYS[3],1); return redis.call('hgetall',KEYS[1]); ", 3, "my_data", "transaction_time", "counter", transactionTime, 300);

        RedisResult[] results = (RedisResult[])result;

        return results.Select(e => (((RedisValue)e).ToString()));
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
