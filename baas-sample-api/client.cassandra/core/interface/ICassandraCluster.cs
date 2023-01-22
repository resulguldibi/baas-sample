using System;

namespace client.cassandra.core
{
    public interface ICassandraCluster : IDisposable
    {
        ICassandraSession Connect(string keyspace);
    }

}
