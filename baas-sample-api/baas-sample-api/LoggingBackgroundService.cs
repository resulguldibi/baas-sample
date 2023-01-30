using System;
using client.kafka.consumer.core;
using Confluent.Kafka;
using System.Threading;
using System.Threading.Tasks;
using client.cassandra.core;
using job_dispatcher.src.main.core.dispatcher;
using job_dispatcher.src.main.core.dispatcher.provider;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static System.Collections.Specialized.BitVector32;
using System.Transactions;
using job_dispatcher.src.main.core.job;
using job_dispatcher.src.main.core.worker;
using System.Text.Json;

namespace baas_sample_api
{
    public class LoggingBackgroundService : IHostedService
    {
        private readonly ILogger<LoggingBackgroundService> _logger;
        private readonly IKafkaConsumerProvider _kafkaConsumerProvider;
        private readonly ICassandraClientProvider _cassandraClientProvider;
        private readonly IDispatcher _logDispatcher;


        public LoggingBackgroundService(IKafkaConsumerProvider kafkaConsumerProvider, ICassandraClientProvider cassandraClientProvider, ILogger<LoggingBackgroundService> logger)
        {
            _logger = logger;
            _kafkaConsumerProvider = kafkaConsumerProvider;
            _cassandraClientProvider = cassandraClientProvider;
            _logDispatcher = DispatcherProvider.GetDispatcher("baas-logging-background-service-dispatcher", 10, 10000);
            _logDispatcher.Start();
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LoggingBackgroundService running.");

            Task.Run(async () =>
            {

                using (IKafkaConsumer<Ignore, string> c = this._kafkaConsumerProvider.GetKafkaConsumer<Ignore, string>("baas-log-consumer"))
                {
                    c.Subscribe();

                    try
                    {
                        while (true)
                        {
                            try
                            {
                                var cr = c.Consume(stoppingToken);
                                LogItem? logItem = null;
                                try
                                {
                                    if (cr != null && cr.Message != null && cr.Message.Value != null && !string.IsNullOrEmpty(cr.Message.Value))
                                    {
                                        logItem = JsonSerializer.Deserialize<LogItem?>(cr.Message.Value);
                                    }

                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Exception occured in KafkaSocketMessage deserialization: {ex.Message}");
                                }


                                try
                                {
                                    if (logItem != null)
                                    {
                                        await _logDispatcher.AddJob(new LoggingBackgroundServiceJob(logItem, this._cassandraClientProvider));
                                    }

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }


                            }
                            catch (ConsumeException e)
                            {
                                _logger.LogError($"Error occured: {e.Error.Reason}");
                            }
                        }
                    }
                    catch (OperationCanceledException e)
                    {
                        _logger.LogError($"OperationCanceledException occured: {e.Message}");
                        c.Close();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Exception occured: {ex.Message}");
                    }
                }

            }, stoppingToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LoggingBackgroundService is stopping.");



            return Task.CompletedTask;
        }
    }

    public class LoggingBackgroundServiceJob : Job, IJob
    {
        private static ICassandraClient cassandraClient;
        public LoggingBackgroundServiceJob(object data, ICassandraClientProvider cassandraClientProvider) : base(data)
        {
            if (cassandraClient == null)
            {
                cassandraClient = cassandraClientProvider.GetCassandraClient("my_cluster", "baas_keyspace");
            }
        }

        public override async Task Do(IWorker worker)
        {
            await Task.Run(() =>
            {
                LogItem logItem = (LogItem)data;

                cassandraClient.Execute($"insert into baas_logs (id , tpp_code, transaction_id, source , controller , action , method , status_code, query_string , request , response , execution_time_ms , message , stack_trace, insert_time, insert_date ) VALUES ('{logItem.Id}','{logItem.TppCode}', '{logItem.TransactionId}', '{logItem.Source}', '{logItem.Controller}', '{logItem.Action}', '{logItem.Method}', {logItem.StatusCode}, '{logItem.QueryString}', '{logItem.Request}', '{logItem.Response}', {logItem.ExecutionTime}, '{logItem.Message}', '{logItem.StackTrace}', {logItem.InsertTime},{logItem.InsertDate})");

            });
        }
    }
}

