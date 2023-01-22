using Cassandra;
using System;
using System.Collections.Generic;

namespace client.cassandra.core
{
    public interface ICassandraClient : IDisposable
    {
        IEnumerable<T> Fetch<T>(string cqlQuery);

        RowSet Execute(string cqlQuery);
    }
}
