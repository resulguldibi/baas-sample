using Microsoft.Extensions.Configuration;

namespace client.cassandra.core
{
    public class CassandraConnectionInfoProvider : ICassandraConnectionInfoProvider
    {
        private readonly IConfiguration configuration;
        public CassandraConnectionInfoProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public ICassandraConnectionInfo GetCassandraConnectionInfo(string id)
        {
            return new CassandraConnectionInfo(configuration[$"{id}:CASSANDRA_HOSTS"].Split(","), int.Parse(configuration[$"{id}:CASSANDRA_PORT"]));
        }
    }
}
