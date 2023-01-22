namespace client.cassandra.core
{
    public class CassandraClientProvider : ICassandraClientProvider
    {
        private readonly ICassandraSessionProvider cassandraSessionProvider;
        public CassandraClientProvider(ICassandraSessionProvider cassandraSessionProvider)
        {
            this.cassandraSessionProvider = cassandraSessionProvider;
        }

        public ICassandraClient GetCassandraClient(string id, string keyspace)
        {
            return new CassandraClient(this.cassandraSessionProvider.GetCassandraSession(id, keyspace));
        }
    }
}
