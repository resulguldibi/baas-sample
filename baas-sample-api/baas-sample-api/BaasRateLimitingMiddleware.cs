using System.Net;
using job_dispatcher.src.main.core.dispatcher;
using job_dispatcher.src.main.core.dispatcher.provider;
using job_dispatcher.src.main.core.job;
using job_dispatcher.src.main.core.worker;
using Microsoft.Extensions.Primitives;

namespace baas_sample_api
{
    public class BaasRateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BaasRateLimitingMiddleware> _logger;
        private readonly IDispatcher _rateLimitingDispatcher;
        private readonly IRateLimitingClient _rateLimitingClient;


        public BaasRateLimitingMiddleware(RequestDelegate next, IRateLimitingClient rateLimitingClient, ILogger<BaasRateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _rateLimitingDispatcher = DispatcherProvider.GetDispatcher("baas-rate-limiting-midlleware-dispatcher", 5, 10000);
            _rateLimitingDispatcher.Start();
            _rateLimitingClient = rateLimitingClient;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //check rate limiting values sync

            string? tppCode = string.Empty;
            StringValues tppCodeValue;
            if (context.Request.Headers.TryGetValue("x-tpp-code", out tppCodeValue))
            {
                tppCode = tppCodeValue.FirstOrDefault();
            }

            RouteData rd = context.Request.HttpContext.GetRouteData();

            string action = string.Empty;

            var actionValue = rd.Values.Where(e => e.Key.Equals("action", StringComparison.Ordinal)).FirstOrDefault();

            if (actionValue.Value != null)
            {
                action = (string)actionValue.Value;
            }

            string controller = string.Empty;

            var controllerValue = rd.Values.Where(e => e.Key.Equals("controller", StringComparison.Ordinal)).FirstOrDefault();

            if (controllerValue.Value != null)
            {
                controller = (string)controllerValue.Value;
            }

            string applicationId = "baas-sample";
            IEnumerable<RateLimitingDefinition> rateLimitingDefinitions = _rateLimitingClient.GetDefinitions(applicationId);

            bool hasLimit;
            long limit = 0;
            long remaining = 0;
            long reset = 0;
            if (rateLimitingDefinitions != null && rateLimitingDefinitions.Count() > 0)
            {
                rateLimitingDefinitions = rateLimitingDefinitions.Where(e => e.TppCode.Equals(tppCode) && e.Controller.Equals(controller) && e.Action.Equals(action) && e.Method.Equals(context.Request.Method) && e.Status);

                if (rateLimitingDefinitions != null && rateLimitingDefinitions.Count() > 0)
                {
                    foreach (RateLimitingDefinition rateLimitingDefinition in rateLimitingDefinitions)
                    {
                        (hasLimit, limit, remaining, reset) = _rateLimitingClient.HasLimit(rateLimitingDefinition);

                        if (!hasLimit)
                        {
                            throw new RateLimitExceedException($"rate limit exceeded for definition : {rateLimitingDefinition.Id}", limit, remaining, reset);
                        }
                    }
                }
            }

            await _next(context);


            if (rateLimitingDefinitions != null && rateLimitingDefinitions.Count() > 0)
            {
                rateLimitingDefinitions = rateLimitingDefinitions.Where(e => e.TppCode.Equals(tppCode) && e.Controller.Equals(controller) && e.Action.Equals(action) && e.Method.Equals(context.Request.Method) && e.Status);

                if (rateLimitingDefinitions != null && rateLimitingDefinitions.Count() > 0)
                {

                    context.Response.Headers.TryAdd("x-rate-limit-limit", limit.ToString());
                    context.Response.Headers.TryAdd("x-rate-limit-remaining", (remaining - 1).ToString());
                    context.Response.Headers.TryAdd("x-rate-limit-reset", reset.ToString());

                    foreach (RateLimitingDefinition rateLimitingDefinition in rateLimitingDefinitions)
                    {
                        try
                        {
                            await _rateLimitingDispatcher.AddJob(new RateLimitingJob(new RateLimitingTransaction()
                            {
                                DefinitionId = rateLimitingDefinition.Id,
                                Id = Cassandra.TimeUuid.NewId(),
                                StatusCode = context.Response.StatusCode,
                                InsertTime = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds,
                                TransactionTime = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds
                            }, _rateLimitingClient));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
        }
    }

    public class RateLimitingJob : Job, IJob
    {
        private readonly IRateLimitingClient _rateLimitingClient;
        public RateLimitingJob(object data, IRateLimitingClient rateLimitingClient) : base(data)
        {
            this._rateLimitingClient = rateLimitingClient;
        }

        public override async Task Do(IWorker worker)
        {
            await Task.Run(() =>
            {
                RateLimitingTransaction rateLimitingTransaction = (RateLimitingTransaction)data;

                _rateLimitingClient.AddTransaction(rateLimitingTransaction);

            });
        }
    }


    public class RateLimitExceedException : Exception
    {
        public int Code => (int)HttpStatusCode.TooManyRequests;
        public long Limit;
        public long Remaining;
        public long Reset;

        public RateLimitExceedException(string message, long limit, long remaining, long reset) : base(message)
        {
            this.Limit = limit;
            this.Remaining = remaining;
            this.Reset = reset;
        }
    }
}

