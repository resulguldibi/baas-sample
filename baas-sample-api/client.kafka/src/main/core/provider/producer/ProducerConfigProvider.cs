using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace client.kafka.producer.core
{
    public class ProducerConfigProvider : IProducerConfigProvider
    {
        private readonly IConfiguration configuration;
        public ProducerConfigProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public IEnumerable<KeyValuePair<string, string>> GetProducerConfig(string id)
        {
            var servers = configuration[$"{id}:kafka-brokers"];
            return new ProducerConfig { BootstrapServers = servers };
        }

        public string GetTopic(string id)
        {
            return configuration.GetSection($"{id}:topic").Get<string>();
        }
    }
}
