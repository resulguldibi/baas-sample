using System.Collections.Generic;

namespace client.kafka.consumer.core
{
    public interface IConsumerConfigProvider
    {
        IEnumerable<KeyValuePair<string, string>> GetConsumerConfig(string id);
        IEnumerable<string> GetTopics(string id);
    }
}
