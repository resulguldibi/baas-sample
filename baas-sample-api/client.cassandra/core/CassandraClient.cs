using Cassandra;
using System.Collections.Generic;

namespace client.cassandra.core
{
    public class CassandraClient : ICassandraClient
    {
        private readonly ICassandraSession cassandraSession;
        public CassandraClient(ICassandraSession cassandraSession)
        {
            this.cassandraSession = cassandraSession;
        }

        public void Dispose()
        {
            this.cassandraSession?.Dispose();
        }

        public RowSet Execute(string cqlQuery)
        {
            return this.cassandraSession.Execute(cqlQuery);
        }

        public IEnumerable<T> Fetch<T>(string cqlQuery)
        {
            return this.cassandraSession.Fetch<T>(cqlQuery);
        }
    }
}
