using Confluent.Kafka;
using System.Collections.Generic;

namespace client.kafka.consumer.core
{
    public class ConsumerBuilderProvider : IConsumerBuilderProvider
    {
        private readonly IConsumerConfigProvider consumerConfigProvider;
        
        public ConsumerBuilderProvider(IConsumerConfigProvider consumerConfigProvider)
        {
            this.consumerConfigProvider = consumerConfigProvider;
        }
              
        public ConsumerBuilder<TKey, TValue> GetConsumerBuilder<TKey, TValue>(string id)
        {             
            return new ConsumerBuilder<TKey, TValue>(this.consumerConfigProvider.GetConsumerConfig(id));
        }

        public IEnumerable<string> GetTopics(string id)
        {
            return this.consumerConfigProvider.GetTopics(id);
        }
    }
}
