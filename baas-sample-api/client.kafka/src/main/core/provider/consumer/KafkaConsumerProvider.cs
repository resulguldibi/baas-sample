namespace client.kafka.consumer.core
{
    public class KafkaConsumerProvider : IKafkaConsumerProvider
    {
        private readonly IConsumerBuilderProvider consumerBuilderProvider;
        public KafkaConsumerProvider(IConsumerBuilderProvider consumerBuilderProvider)
        {
            this.consumerBuilderProvider = consumerBuilderProvider;
        }
        public IKafkaConsumer<TKey, TValue> GetKafkaConsumer<TKey, TValue>(string id)
        {
            var builder = this.consumerBuilderProvider.GetConsumerBuilder<TKey, TValue>(id);
            var topics = this.consumerBuilderProvider.GetTopics(id);
            return new KafkaConsumer<TKey, TValue>(builder.Build(), topics);
        }
    }

}
