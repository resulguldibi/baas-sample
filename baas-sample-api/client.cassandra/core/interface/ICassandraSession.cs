using Cassandra;
using System;
using System.Collections.Generic;

namespace client.cassandra.core
{
    public interface ICassandraSession : IDisposable
    {
        RowSet Execute(string cqlQuery);

        IEnumerable<T> Fetch<T>(string cqlQuery);

    }

}
