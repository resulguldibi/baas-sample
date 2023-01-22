using Cassandra;
using Cassandra.Mapping;
using System.Collections.Generic;

namespace client.cassandra.core
{
    public class CassandraSession : ICassandraSession
    {
        private readonly ISession session;
        public CassandraSession(ISession session)
        {
            this.session = session;
        }
        public void Dispose()
        {
            this.session?.Dispose();
        }

        public RowSet Execute(string cqlQuery)
        {
            return this.session.Execute(cqlQuery);
        }

        public IEnumerable<T> Fetch<T>(string cqlQuery)
        {
            IMapper mapper = new Mapper(this.session);

            return mapper.Fetch<T>(cqlQuery);
        }
    }
}
