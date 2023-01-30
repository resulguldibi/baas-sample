using System;
using System.Text.Json;
using client.cassandra.core;
using client.kafka.producer.core;
using Confluent.Kafka;

namespace baas_sample_api
{
    public class CassandraLoggingClient : ILoggingClient
    {
        private static ICassandraClient cassandraClient;
        public CassandraLoggingClient(ICassandraClientProvider cassandraClientProvider)
        {
            if (cassandraClient == null)
            {
                cassandraClient = cassandraClientProvider.GetCassandraClient("my_cluster", "baas_keyspace");
            }
        }
        public void Log(LogItem logItem)
        {
            cassandraClient.Execute($"insert into baas_logs (id , tpp_code, transaction_id, source , controller , action , method , status_code, query_string , request , response , execution_time_ms , message , stack_trace, insert_time, insert_date ) VALUES ('{logItem.Id}','{logItem.TppCode}', '{logItem.TransactionId}', '{logItem.Source}', '{logItem.Controller}', '{logItem.Action}', '{logItem.Method}', {logItem.StatusCode}, '{logItem.QueryString}', '{logItem.Request}', '{logItem.Response}', {logItem.ExecutionTime}, '{logItem.Message}', '{logItem.StackTrace}', {logItem.InsertTime},{logItem.InsertDate})");
        }
    }


    public class KafkaLoggingClient : ILoggingClient
    {
        private static IKafkaProducer<Null, string> kafkaProducer;
        public KafkaLoggingClient(IKafkaProducerProvider kafkaProducerProvider)
        {
            if (kafkaProducer == null)
            {
                kafkaProducer = kafkaProducerProvider.GetKafkaProducer<Null, string>("baas-log-producer");
            }
        }
        public void Log(LogItem logItem)
        {

            DeliveryResult<Null, string> dr = kafkaProducer.ProduceAsync(new Message<Null, string> { Value = JsonSerializer.Serialize(logItem) }).Result;

        }
    }


    public interface ILoggingClient
    {
        void Log(LogItem logItem);
    }
}

