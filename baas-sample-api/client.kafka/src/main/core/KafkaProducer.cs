using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace client.kafka.producer.core
{
    public class KafkaProducer<TKey, TValue> : IKafkaProducer<TKey, TValue>
    {
        private readonly IProducer<TKey, TValue> producer;
        private readonly string topic;
        public KafkaProducer(IProducer<TKey, TValue> producer, string topic)
        {
            this.producer = producer;
            this.topic = topic;
        }

        public void Dispose()
        {
            this.producer?.Dispose();
        }

        public int Flush(TimeSpan timeout)
        {
            return this.producer.Flush(timeout);
        }

        public void Produce(Message<TKey, TValue> message, Action<DeliveryReport<TKey, TValue>> deliveryHandler = null)
        {
            this.producer.Produce(this.topic, message, deliveryHandler);
        }

        public async Task<DeliveryResult<TKey, TValue>> ProduceAsync(Message<TKey, TValue> message, CancellationToken cancellationToken = default)
        {
            return await this.producer.ProduceAsync(this.topic,message, cancellationToken);
        }
    }
}
