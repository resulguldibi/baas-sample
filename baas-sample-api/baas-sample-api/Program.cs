using baas_sample_api;
using Cassandra.Mapping;
using client.cassandra.core;
using client.kafka.consumer.core;
using client.kafka.producer.core;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IIdempotemcyClient, IdempotemcyViaRedisClient>();
//builder.Services.AddSingleton<ILoggingClient, CassandraLoggingClient>();
builder.Services.AddSingleton<ILoggingClient, KafkaLoggingClient>();
builder.Services.AddSingleton<IRateLimitingClient, CassandraRateLimitingClient>();
builder.Services.AddSingleton<IQuotaManagementClient, CassandraQuotaManagementClient>();
builder.Services.AddSingleton<ICassandraClientProvider, CassandraClientProvider>();
builder.Services.AddSingleton<ICassandraSessionProvider, CassandraSessionProvider>();
builder.Services.AddSingleton<ICassandraClusterProvider, CassandraClusterProvider>();
builder.Services.AddSingleton<ICassandraConnectionInfoProvider, CassandraConnectionInfoProvider>();
builder.Services.AddSingleton<IKafkaProducerProvider, KafkaProducerProvider>();
builder.Services.AddSingleton<IProducerBuilderProvider, ProducerBuilderProvider>();
builder.Services.AddSingleton<IProducerConfigProvider, ProducerConfigProvider>();
builder.Services.AddSingleton<IKafkaConsumerProvider, KafkaConsumerProvider>();
builder.Services.AddSingleton<IConsumerBuilderProvider, ConsumerBuilderProvider>();
builder.Services.AddSingleton<IConsumerConfigProvider, ConsumerConfigProvider>();


builder.Services.AddHostedService<LoggingBackgroundService>();

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
MiddlewareExtension.UseBaasConsent(app);
MiddlewareExtension.UseBaasRatelimiting(app);
MiddlewareExtension.UseBaasQuotaManagement(app);
MiddlewareExtension.UseBaasIdempotemcy(app);
MiddlewareExtension.UseBaasDataIntegrity(app);

app.MapControllers();

MappingConfiguration.Global.Define(
   new Map<RateLimitingDefinition>()
      .TableName("baas_rate_limit_definitions")
      .PartitionKey(new string[] { "tpp_code", "application", "controller", "action", "method", "status" })
      .Column(u => u.Id, cm => cm.WithName("id"))
      .Column(u => u.TppCode, cm => cm.WithName("tpp_code"))
      .Column(u => u.Application, cm => cm.WithName("application"))
      .Column(u => u.Controller, cm => cm.WithName("controller"))
      .Column(u => u.Action, cm => cm.WithName("action"))
      .Column(u => u.Method, cm => cm.WithName("method"))
      .Column(u => u.LimitPeriod, cm => cm.WithName("limit_period"))
      .Column(u => u.LimitCount, cm => cm.WithName("limit_count"))
      .Column(u => u.Status, cm => cm.WithName("status")),

   new Map<RateLimitingTransaction>()
      .TableName("baas_rate_limit_transactions")
      .PartitionKey(new string[] { "definition_id", "status_code" })
      .Column(u => u.Id, cm => cm.WithName("id"))
      .Column(u => u.DefinitionId, cm => cm.WithName("definition_id"))
      .Column(u => u.InsertTime, cm => cm.WithName("insert_time"))
      .Column(u => u.StatusCode, cm => cm.WithName("status_code"))
      .Column(u => u.TransactionTime, cm => cm.WithName("transaction_time")),


   new Map<QuotaDefinition>()
      .TableName("baas_quota_definitions")
      .PartitionKey(new string[] { "tpp_code", "application", "controller", "action", "method", "status" })
      .Column(u => u.Id, cm => cm.WithName("id"))
      .Column(u => u.TppCode, cm => cm.WithName("tpp_code"))
      .Column(u => u.Application, cm => cm.WithName("application"))
      .Column(u => u.Controller, cm => cm.WithName("controller"))
      .Column(u => u.Action, cm => cm.WithName("action"))
      .Column(u => u.Method, cm => cm.WithName("method"))
      .Column(u => u.QuotaPeriod, cm => cm.WithName("quota_period"))
      .Column(u => u.QuotaKeySourceType, cm => cm.WithName("quota_key_source_type"))
      .Column(u => u.QuotaKeySourceName, cm => cm.WithName("quota_key_source_name"))
      .Column(u => u.QuotaCount, cm => cm.WithName("quota_count"))
      .Column(u => u.Status, cm => cm.WithName("status")),


     new Map<QuotaTransaction>()
      .TableName("baas_quota_transactions")
      .PartitionKey(new string[] { "definition_id", "status_code" })
      .Column(u => u.Id, cm => cm.WithName("id"))
      .Column(u => u.DefinitionId, cm => cm.WithName("definition_id"))
      .Column(u => u.InsertTime, cm => cm.WithName("insert_time"))
      .Column(u => u.StatusCode, cm => cm.WithName("status_code"))
      .Column(u => u.TransactionTime, cm => cm.WithName("transaction_time"))
      .Column(u => u.QuotaKeySourceValue, cm => cm.WithName("quota_key_source_value"))

      );



app.Run("http://*:8080");



