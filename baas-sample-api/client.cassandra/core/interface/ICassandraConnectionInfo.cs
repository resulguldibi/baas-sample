namespace client.cassandra.core
{
    public interface ICassandraConnectionInfo
    {
        string[] GetHosts();
        int GetPort();
    }
}
