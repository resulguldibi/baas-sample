using Confluent.Kafka;
using System.Collections.Generic;

namespace client.kafka.consumer.core
{
    public interface IConsumerBuilderProvider
    {
        ConsumerBuilder<TKey, TValue> GetConsumerBuilder<TKey, TValue>(string id);

        IEnumerable<string> GetTopics(string id);
    }
}
