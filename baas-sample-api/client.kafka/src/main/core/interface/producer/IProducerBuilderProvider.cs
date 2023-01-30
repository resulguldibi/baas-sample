using Confluent.Kafka;

namespace client.kafka.producer.core
{
    public interface IProducerBuilderProvider
    {
        ProducerBuilder<TKey, TValue> GetProducerBuilder<TKey, TValue>(string id);
        string GetTopic(string id);
    }
}
