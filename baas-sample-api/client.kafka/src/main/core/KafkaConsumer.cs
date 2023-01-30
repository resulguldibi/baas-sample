using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace client.kafka.consumer.core
{
    public class KafkaConsumer<TKey, TValue> : IKafkaConsumer<TKey, TValue>
    {
        private readonly IConsumer<TKey, TValue> consumer;
        private readonly IEnumerable<string> topics;
        public KafkaConsumer(IConsumer<TKey, TValue> consumer,IEnumerable<string> topics)
        {
            this.consumer = consumer;
            this.topics = topics;
        }

        

        public void Close()
        {
            this.consumer.Close();
        }

        public ConsumeResult<TKey, TValue> Consume(CancellationToken cancellationToken = default)
        {
            return this.consumer.Consume(cancellationToken);
        }

        public ConsumeResult<TKey, TValue> Consume(TimeSpan timeout)
        {
            return this.consumer.Consume(timeout);
        }

        public ConsumeResult<TKey, TValue> Consume(int millisecondsTimeout)
        {
            return this.consumer.Consume(millisecondsTimeout);
        }

        public void Dispose()
        {
            this.consumer?.Dispose();
        }

        public void Subscribe()
        {
            this.consumer.Subscribe(this.topics);
        }

        public void Assign(string topic, int partition)
        {
            if(topic == null || topic.Equals(string.Empty))
            {
                throw new ArgumentNullException("topic", "topic is null or empty");
            }

            if(partition < 0)
            {
                throw new ArgumentOutOfRangeException("partition", "partition must be greater than 0.");
            }

            if (!this.topics.Contains(topic))
            {
                throw new ArgumentOutOfRangeException("topic", "topic not in subscribed topic list");
            }

            this.consumer.Assign(new TopicPartition(topic, new Partition(partition)));
        }
    }
}
