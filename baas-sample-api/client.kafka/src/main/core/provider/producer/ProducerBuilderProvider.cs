using Confluent.Kafka;
using System.Collections.Generic;

namespace client.kafka.producer.core
{
    public class ProducerBuilderProvider : IProducerBuilderProvider
    {
        private readonly IProducerConfigProvider producerConfigProvider;
        public ProducerBuilderProvider(IProducerConfigProvider producerConfigProvider)
        {
            this.producerConfigProvider = producerConfigProvider;
        }

        public ProducerBuilder<TKey, TValue> GetProducerBuilder<TKey, TValue>(string id)
        {
            return new ProducerBuilder<TKey, TValue>(this.producerConfigProvider.GetProducerConfig(id));
        }

        public string GetTopic(string id)
        {
            return this.producerConfigProvider.GetTopic(id);
        }
    }
}
