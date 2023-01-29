using System.Text.Json;
using StackExchange.Redis;

namespace baas_sample_api
{
    public class IdempotemcyViaRedisClient : IIdempotemcyClient
    {
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IDatabase cache;

        public IdempotemcyViaRedisClient(IConnectionMultiplexer connectionMultiplexer)
        {
            this.connectionMultiplexer = connectionMultiplexer;
            this.cache = this.connectionMultiplexer.GetDatabase();
        }

        public (bool, IdempotentData?) IsIdempotent(string idempotemcyKey, string randomValue)
        {
            #region check is transaction idempotent

            RedisValue idempotentData = RedisValue.Null;
            try
            {
                idempotentData = this.cache.StringGet(idempotemcyKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (!string.IsNullOrEmpty(idempotentData) && !idempotentData.IsNull && idempotentData.HasValue)
            {
                return (true, JsonSerializer.Deserialize<IdempotentData>(idempotentData.ToString()));
            }
            else
            {
                #region check is transaction-id in use
                RedisResult? redisResult = null;
                try
                {
                    redisResult = this.cache.Execute("SET", new object[] { $"{idempotemcyKey}|LOCK", $"{randomValue}", "NX", "PX", 30000 });

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                if (redisResult != null && redisResult.IsNull)
                {
                    throw new Exception("transaction is in use");
                }

                #endregion

                return (false, null);
            }

            #endregion

        }

        public void MarkAsIdempotent(string idempotemcyKey, string randomValue, IdempotentData idempotentData, long idempotemcyRetentionMs)
        {
            try
            {
                #region store idempotent data
                this.cache.StringSet(idempotemcyKey, JsonSerializer.Serialize(idempotentData), TimeSpan.FromMilliseconds(idempotemcyRetentionMs));
                #endregion

                #region release lock
                RedisValue idempotemcyLock = this.cache.StringGet($"{idempotemcyKey}|LOCK");

                if (!string.IsNullOrEmpty(idempotemcyLock) && !idempotemcyLock.IsNull && idempotemcyLock.HasValue && idempotemcyLock.Equals(randomValue))
                {
                    this.cache.KeyDelete($"{idempotemcyKey}|LOCK");
                }

                #endregion

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    /*
    public class IdempotemcyViaInMemoryCacheClient : IIdempotemcyClient
    {
        private readonly IMemoryCache memoryCache;
        public IdempotemcyViaInMemoryCacheClient(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }



        public (bool, IdempotentData?) IsIdempotent(string idempotemcyKey)
        {

            IdempotentData? idempotentData = this.memoryCache.Get<IdempotentData>(idempotemcyKey);

            if (idempotentData == null)
            {
                return (false, idempotentData);
            }
            else
            {
                return (true, idempotentData);
            }
        }

        public void MarkAsIdempotent(string idempotemcyKey, string idempotentData, int idempotentStatusCode, string idempotentContentType, long idempotemcyRetentionMs)
        {

            var data = new IdempotentData()
            {
                ContentType = idempotentContentType,
                Body = idempotentData,
                StatusCode = idempotentStatusCode
            };

            this.memoryCache.Set<IdempotentData>(idempotemcyKey, data, TimeSpan.FromMilliseconds(idempotemcyRetentionMs));
        }
    }



    public class IdempotemcyViaCassandraClient : IIdempotemcyClient
    {
        public IdempotemcyViaCassandraClient()
        {
        }



        public (bool, IdempotentData?) IsIdempotent(string idempotemcyKey)
        {
            return (true, new IdempotentData());
        }

        public void MarkAsIdempotent(string idempotemcyKey, string idempotentData, int idempotentStatusCode, string idempotentContentType, long idempotemcyRetentionMs)
        {
            //store data to cassandra
        }
    }

    */

    public interface IIdempotemcyClient
    {
        public (bool, IdempotentData?) IsIdempotent(string idempotemcyKey, string randomValue);
        public void MarkAsIdempotent(string idempotemcyKey, string randomValue, IdempotentData idempotentData, long idempotemcyRetentionMs);
    }

    public class IdempotentData
    {
        public string ContentType { get; set; }
        public int StatusCode { get; set; }
        public string Body { get; set; }
    }
}

