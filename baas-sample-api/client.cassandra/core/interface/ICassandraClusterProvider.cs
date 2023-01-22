namespace client.cassandra.core
{
    public interface ICassandraClusterProvider
    {
        ICassandraCluster GetCassandraCluster(string id);
    }
}
