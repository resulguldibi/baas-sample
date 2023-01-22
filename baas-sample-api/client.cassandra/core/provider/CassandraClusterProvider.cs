namespace client.cassandra.core
{
    public class CassandraClusterProvider : ICassandraClusterProvider
    {
        private readonly ICassandraConnectionInfoProvider cassandraConnectionInfoProvider;
        public CassandraClusterProvider(ICassandraConnectionInfoProvider cassandraConnectionInfoProvider)
        {
            this.cassandraConnectionInfoProvider = cassandraConnectionInfoProvider;
        }

        public ICassandraCluster GetCassandraCluster(string id)
        {
            return new CassandraCluster(this.cassandraConnectionInfoProvider.GetCassandraConnectionInfo(id));
        }
    }
}
