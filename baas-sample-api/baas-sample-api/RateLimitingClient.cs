using System;
using System.Text.Json.Serialization;
using Cassandra;
using client.cassandra.core;

namespace baas_sample_api
{
    public class CassandraRateLimitingClient : IRateLimitingClient
    {
        private static ICassandraClient cassandraClient;
        public CassandraRateLimitingClient(ICassandraClientProvider cassandraClientProvider)
        {
            if (cassandraClient == null)
            {
                cassandraClient = cassandraClientProvider.GetCassandraClient("my_cluster", "baas_keyspace");
            }
        }

        public void AddTransaction(RateLimitingTransaction rateLimitingTransaction)
        {
            cassandraClient.Execute($"insert into baas_rate_limit_transactions (id, definition_id, status_code, insert_time, transaction_time) VALUES ({rateLimitingTransaction.Id},{rateLimitingTransaction.DefinitionId}, {rateLimitingTransaction.StatusCode}, {rateLimitingTransaction.InsertTime}, {rateLimitingTransaction.TransactionTime})");
        }

        public IEnumerable<RateLimitingDefinition> GetDefinitions(string application)
        {
            return cassandraClient.Fetch<RateLimitingDefinition>($"select * from baas_rate_limit_definitions where application = '{application}' allow filtering;").ToList();
        }

        public (bool, long, long, long) HasLimit(RateLimitingDefinition rateLimitingDefinition)
        {
            int referenceTime = (int)(DateTime.Now.AddSeconds(rateLimitingDefinition.LimitPeriod * -1) - new DateTime(1970, 1, 1)).TotalSeconds;
            IEnumerable<RateLimitingTransaction> results = cassandraClient.Fetch<RateLimitingTransaction>($"select * from baas_rate_limit_transactions where definition_id = {rateLimitingDefinition.Id} and transaction_time >= {referenceTime} order by transaction_time asc limit {rateLimitingDefinition.LimitCount}").ToList();

            if (results != null && results.Count() >= rateLimitingDefinition.LimitCount)
            {

                return (false, rateLimitingDefinition.LimitCount, 0, (int)TimeSpan.FromSeconds(results.First().TransactionTime + rateLimitingDefinition.LimitPeriod).TotalSeconds);
            }
            else
            {
                return (true, rateLimitingDefinition.LimitCount, (rateLimitingDefinition.LimitCount - (results != null && results.Any() ? results.Count() : 0)), 0);
            }
        }
    }

    public interface IRateLimitingClient
    {
        void AddTransaction(RateLimitingTransaction rateLimitingTransaction);
        (bool, long, long, long) HasLimit(RateLimitingDefinition rateLimitingDefinition);
        IEnumerable<RateLimitingDefinition> GetDefinitions(string application);
    }

    public class RateLimitingDefinition
    {
        [JsonPropertyName("id")]
        public TimeUuid Id { get; set; }
        [JsonPropertyName("tpp_code")]
        public string TppCode { get; set; }
        [JsonPropertyName("application")]
        public string Application { get; set; }
        [JsonPropertyName("controller")]
        public string Controller { get; set; }
        [JsonPropertyName("action")]
        public string Action { get; set; }
        [JsonPropertyName("method")]
        public string Method { get; set; }
        [JsonPropertyName("limit_period")]
        public long LimitPeriod { get; set; }
        [JsonPropertyName("limit_count")]
        public long LimitCount { get; set; }
        [JsonPropertyName("status")]
        public bool Status { get; set; }

    }

    public class RateLimitingTransaction
    {
        [JsonPropertyName("id")]
        public TimeUuid Id { get; set; }
        [JsonPropertyName("definition_id")]
        public TimeUuid DefinitionId { get; set; }
        [JsonPropertyName("status_code")]
        public int StatusCode { get; set; }
        [JsonPropertyName("insert_time")]
        public long InsertTime { get; set; }
        [JsonPropertyName("transaction_time")]
        public long TransactionTime { get; set; }
    }
}

