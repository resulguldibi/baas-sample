using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;


namespace client.kafka.consumer.core
{
    public class ConsumerConfigProvider : IConsumerConfigProvider
    {
        private readonly IConfiguration configuration;    
        public ConsumerConfigProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public IEnumerable<KeyValuePair<string, string>> GetConsumerConfig(string id)
        {

            var servers = configuration[$"{id}:kafka-brokers"];
            var groupId = configuration[$"{id}:group-id"];
            var autoOffsetReset = AutoOffsetReset.Earliest;

            Enum.TryParse<AutoOffsetReset>(configuration[$"{id}:auto-offset-reset"], out autoOffsetReset);

            return new ConsumerConfig { BootstrapServers = servers, GroupId = groupId, AutoOffsetReset = autoOffsetReset };
        }

        public IEnumerable<string> GetTopics(string id)
        {
            return configuration.GetSection($"{id}:topics").Get<string[]>();
        }
    }
}
