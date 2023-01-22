using Cassandra;

namespace client.cassandra.core
{
    public class CassandraCluster : ICassandraCluster
    {
        private readonly Cluster cluster;
        public CassandraCluster(ICassandraConnectionInfo cassandraConnectionInfo)
        {
            this.cluster = Cluster.Builder()
                     .AddContactPoints(cassandraConnectionInfo.GetHosts())
                     .WithPort(cassandraConnectionInfo.GetPort())
                     .Build();
        }
        public ICassandraSession Connect(string keyspace)
        {
            return new CassandraSession(cluster.Connect(keyspace));
        }

        public void Dispose()
        {
            this.cluster.Dispose();
        }
    }

}
