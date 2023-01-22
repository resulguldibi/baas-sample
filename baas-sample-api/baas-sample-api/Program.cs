using baas_sample_api;
using client.cassandra.core;
using job_dispatcher.src.main.core.dispatcher;
using job_dispatcher.src.main.core.dispatcher.provider;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IIdempotemcyClient, IdempotemcyViaRedisClient>();

builder.Services.AddSingleton<ILoggingClient, CassandraLoggingClient>();

builder.Services.AddSingleton<ICassandraClientProvider, CassandraClientProvider>();

builder.Services.AddSingleton<ICassandraSessionProvider, CassandraSessionProvider>();

builder.Services.AddSingleton<ICassandraClusterProvider, CassandraClusterProvider>();

builder.Services.AddSingleton<ICassandraConnectionInfoProvider, CassandraConnectionInfoProvider>();



var multiplexer = ConnectionMultiplexer.Connect("redis:6379,abortConnect=false");
builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
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

MiddlewareExtension.UseBaasExceptionHandling(app);
MiddlewareExtension.UseBaasLogging(app);
MiddlewareExtension.UseBaasAuthentication(app);
MiddlewareExtension.UseBaasAuthorization(app);
MiddlewareExtension.UseBaasRatelimiting(app);
MiddlewareExtension.UseBaasIdempotemcy(app);
MiddlewareExtension.UseBaasDataIntegrity(app);

app.MapControllers();


app.Run("http://*:8080");



