namespace client.kafka.consumer.core
{
    public interface IKafkaConsumerProvider
    {
        IKafkaConsumer<TKey, TValue> GetKafkaConsumer<TKey, TValue>(string id);
    }
}
