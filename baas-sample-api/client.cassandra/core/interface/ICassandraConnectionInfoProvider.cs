namespace client.cassandra.core
{
    public interface ICassandraConnectionInfoProvider
    {
        ICassandraConnectionInfo GetCassandraConnectionInfo(string id);
    }
}
