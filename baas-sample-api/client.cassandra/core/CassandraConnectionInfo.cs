namespace client.cassandra.core
{
    public class CassandraConnectionInfo : ICassandraConnectionInfo
    {
        private readonly string[] hosts;
        private readonly int port;
        public CassandraConnectionInfo(string[] hosts, int port)
        {
            this.hosts = hosts;
            this.port = port;
        }

        public string[] GetHosts()
        {
            return this.hosts;
        }

        public int GetPort()
        {
            return this.port;
        }
    }
}
