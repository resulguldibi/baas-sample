namespace client.kafka.producer.core
{
    public class KafkaProducerProvider : IKafkaProducerProvider
    {

        private readonly IProducerBuilderProvider producerBuilderProvider;
        public KafkaProducerProvider(IProducerBuilderProvider producerBuilderProvider)
        {
            this.producerBuilderProvider = producerBuilderProvider;
        }
        public IKafkaProducer<TKey, TValue> GetKafkaProducer<TKey, TValue>(string id)
        {
            var builder = this.producerBuilderProvider.GetProducerBuilder<TKey, TValue>(id);
            var topic = this.producerBuilderProvider.GetTopic(id);

            return new KafkaProducer<TKey, TValue>(builder.Build(),topic);
        }
    }
}
