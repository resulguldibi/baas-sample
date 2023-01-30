namespace client.kafka.producer.core
{
    public interface IKafkaProducerProvider
    {
        IKafkaProducer<TKey, TValue> GetKafkaProducer<TKey, TValue>(string id);
    }
}
