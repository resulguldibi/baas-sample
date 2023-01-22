namespace client.cassandra.core
{
    public interface ICassandraSessionProvider
    {
        ICassandraSession GetCassandraSession(string id, string keyspace);
    }
}
