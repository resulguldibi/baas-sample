using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Cassandra;
using client.cassandra.core;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace baas_sample_api
{
    public class CassandraQuotaManagementClient : IQuotaManagementClient
    {
        private static ICassandraClient cassandraClient;
        public CassandraQuotaManagementClient(ICassandraClientProvider cassandraClientProvider)
        {
            if (cassandraClient == null)
            {
                cassandraClient = cassandraClientProvider.GetCassandraClient("my_cluster", "baas_keyspace");
            }
        }

        public void AddTransaction(QuotaTransaction quotaTransaction)
        {
            cassandraClient.Execute($"insert into baas_quota_transactions (id, definition_id, quota_key_source_value, status_code, insert_time, transaction_time) VALUES ({quotaTransaction.Id},{quotaTransaction.DefinitionId}, '{quotaTransaction.QuotaKeySourceValue}', {quotaTransaction.StatusCode}, {quotaTransaction.InsertTime}, {quotaTransaction.TransactionTime})");
        }

        public IEnumerable<QuotaDefinition> GetDefinitions(string application)
        {
            return cassandraClient.Fetch<QuotaDefinition>($"select * from baas_quota_definitions where application = '{application}' allow filtering;").ToList();
        }

        public (bool, long, long, long, string?) HasQuota(QuotaDefinition quotaDefinition, HttpContext context)
        {
            #region get quota source key value
            string? quotaKeySourceValue = string.Empty;

            if (quotaDefinition.QuotaKeySourceType.Equals("header"))
            {
                StringValues quotaKeySourceNameValue;

                if (context.Request.Headers.TryGetValue(quotaDefinition.QuotaKeySourceName, out quotaKeySourceNameValue))
                {
                    quotaKeySourceValue = quotaKeySourceNameValue.FirstOrDefault();
                }
            }
            else if (quotaDefinition.QuotaKeySourceType.Equals("route"))
            {
                RouteData rd = context.Request.HttpContext.GetRouteData();

                var routeValue = rd.Values.Where(e => e.Key.Equals(quotaDefinition.QuotaKeySourceName, StringComparison.Ordinal)).FirstOrDefault();

                if (routeValue.Value != null)
                {
                    quotaKeySourceValue = (string)routeValue.Value;
                }
            }
            else if (quotaDefinition.QuotaKeySourceType.Equals("query"))
            {
                StringValues quotaKeySourceNameValue;

                if (context.Request.Query.TryGetValue(quotaDefinition.QuotaKeySourceName, out quotaKeySourceNameValue))
                {
                    quotaKeySourceValue = quotaKeySourceNameValue.FirstOrDefault();
                }
            }
            else if (quotaDefinition.QuotaKeySourceType.Equals("body"))
            {
                #region read request body

                context.Request.EnableBuffering();

                var stream = context.Request.Body;

                string requestBodyText = string.Empty;

                using (var reader = new StreamReader(stream))
                {
                    requestBodyText = reader.ReadToEndAsync().Result;

                    requestBodyText = Regex.Replace(requestBodyText, @"\t|\n|\r", "");

                    byte[] bodyData = Encoding.UTF8.GetBytes(requestBodyText);

                    context.Request.Body = new MemoryStream(bodyData);
                }

                JObject parsedJson = JObject.Parse(requestBodyText);

                quotaKeySourceValue = (string)parsedJson.SelectToken(quotaDefinition.QuotaKeySourceName, true);

                #endregion
            }


            #endregion

            int referenceTime = (int)(DateTime.Now.AddSeconds(quotaDefinition.QuotaPeriod * -1) - new DateTime(1970, 1, 1)).TotalSeconds;

            IEnumerable<QuotaTransaction> results = cassandraClient.Fetch<QuotaTransaction>($"select * from baas_quota_transactions where definition_id = {quotaDefinition.Id} and quota_key_source_value = '{quotaKeySourceValue}' and transaction_time >= {referenceTime} order by transaction_time asc limit {quotaDefinition.QuotaCount}").ToList();

            if (results != null && results.Count() >= quotaDefinition.QuotaCount)
            {
                return (false, quotaDefinition.QuotaCount, 0, (int)TimeSpan.FromSeconds(results.First().TransactionTime + quotaDefinition.QuotaPeriod).TotalSeconds, quotaKeySourceValue);
            }
            else
            {
                return (true, quotaDefinition.QuotaCount, (quotaDefinition.QuotaCount - (results != null && results.Any() ? results.Count() : 0)), 0, quotaKeySourceValue);
            }
        }
    }

    public interface IQuotaManagementClient
    {
        void AddTransaction(QuotaTransaction quotaTransaction);
        (bool, long, long, long, string?) HasQuota(QuotaDefinition quotaDefinition, HttpContext context);
        IEnumerable<QuotaDefinition> GetDefinitions(string application);
    }

    public class QuotaDefinition
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
        [JsonPropertyName("quota_period")]
        public long QuotaPeriod { get; set; }
        [JsonPropertyName("quota_key_source_type")]
        public string QuotaKeySourceType { get; set; }
        [JsonPropertyName("quota_key_source_name")]
        public string QuotaKeySourceName { get; set; }
        [JsonPropertyName("quota_count")]
        public long QuotaCount { get; set; }
        [JsonPropertyName("status")]
        public bool Status { get; set; }

    }

    public class QuotaTransaction
    {
        [JsonPropertyName("id")]
        public TimeUuid Id { get; set; }
        [JsonPropertyName("definition_id")]
        public TimeUuid DefinitionId { get; set; }
        [JsonPropertyName("status_code")]
        public int StatusCode { get; set; }
        [JsonPropertyName("quota_key_source_value")]
        public string QuotaKeySourceValue { get; set; }
        [JsonPropertyName("insert_time")]
        public long InsertTime { get; set; }
        [JsonPropertyName("transaction_time")]
        public long TransactionTime { get; set; }
    }
}

