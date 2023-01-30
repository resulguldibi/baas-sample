using System.Collections.Generic;

namespace client.kafka.producer.core
{
    public interface IProducerConfigProvider
    {
        IEnumerable<KeyValuePair<string, string>> GetProducerConfig(string id);

        string GetTopic(string id);
    }
}
