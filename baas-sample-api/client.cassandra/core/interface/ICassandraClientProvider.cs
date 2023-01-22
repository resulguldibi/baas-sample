namespace client.cassandra.core
{
    public interface ICassandraClientProvider
    {
        ICassandraClient GetCassandraClient(string id, string keyspace);
    }
}
