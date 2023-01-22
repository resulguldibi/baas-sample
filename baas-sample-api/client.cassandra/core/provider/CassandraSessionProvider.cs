namespace client.cassandra.core
{
    public class CassandraSessionProvider : ICassandraSessionProvider
    {
        private readonly ICassandraClusterProvider cassandraClusterProvider;
        private ICassandraCluster cassandraCluster;
        public CassandraSessionProvider(ICassandraClusterProvider cassandraClusterProvider)
        {
            this.cassandraClusterProvider = cassandraClusterProvider;
        }
        public ICassandraSession GetCassandraSession(string id, string keyspace)
        {
            if (cassandraCluster == null)
            {
                cassandraCluster = this.cassandraClusterProvider.GetCassandraCluster(id);
            }

            return this.cassandraCluster.Connect(keyspace);
        }
    }
}
