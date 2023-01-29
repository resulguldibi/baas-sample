using System.Net;
using job_dispatcher.src.main.core.dispatcher;
using job_dispatcher.src.main.core.dispatcher.provider;
using job_dispatcher.src.main.core.job;
using job_dispatcher.src.main.core.worker;
using Microsoft.Extensions.Primitives;

namespace baas_sample_api
{
    public class BaasQuotaManagementMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BaasRateLimitingMiddleware> _logger;
        private readonly IDispatcher _quotaManagementDispatcher;
        private readonly IQuotaManagementClient _quotaManagementClient;


        public BaasQuotaManagementMiddleware(RequestDelegate next, IQuotaManagementClient quotaManagementClient, ILogger<BaasRateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _quotaManagementDispatcher = DispatcherProvider.GetDispatcher("baas-quota-management-midlleware-dispatcher", 5, 10000);
            _quotaManagementDispatcher.Start();
            _quotaManagementClient = quotaManagementClient;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //check quota limit values sync

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
            IEnumerable<QuotaDefinition> quotaDefinitions = _quotaManagementClient.GetDefinitions(applicationId);

            bool hasLimit;
            long limit = 0;
            long remaining = 0;
            long reset = 0;
            string? quotaKeySourceValue = string.Empty;
            if (quotaDefinitions != null && quotaDefinitions.Count() > 0)
            {
                quotaDefinitions = quotaDefinitions.Where(e => e.TppCode.Equals(tppCode) && e.Controller.Equals(controller) && e.Action.Equals(action) && e.Method.Equals(context.Request.Method) && e.Status);

                if (quotaDefinitions != null && quotaDefinitions.Count() > 0)
                {
                    foreach (QuotaDefinition quotaDefinition in quotaDefinitions)
                    {
                        (hasLimit, limit, remaining, reset, quotaKeySourceValue) = _quotaManagementClient.HasQuota(quotaDefinition, context);

                        if (!hasLimit)
                        {
                            throw new QuotaExceedException($"quota exceeded for definition : {quotaDefinition.Id}", limit, remaining, reset);
                        }
                    }
                }
            }

            await _next(context);


            if (quotaDefinitions != null && quotaDefinitions.Count() > 0)
            {
                quotaDefinitions = quotaDefinitions.Where(e => e.TppCode.Equals(tppCode) && e.Controller.Equals(controller) && e.Action.Equals(action) && e.Method.Equals(context.Request.Method) && e.Status);

                if (quotaDefinitions != null && quotaDefinitions.Count() > 0)
                {

                    context.Response.Headers.TryAdd("x-quota-limit-limit", limit.ToString());
                    context.Response.Headers.TryAdd("x-quota-limit-remaining", (remaining - 1).ToString());
                    context.Response.Headers.TryAdd("x-quota-limit-reset", reset.ToString());

                    foreach (QuotaDefinition quotaDefinition in quotaDefinitions)
                    {
                        try
                        {
                            await _quotaManagementDispatcher.AddJob(new QuotaManagementJob(new QuotaTransaction()
                            {
                                DefinitionId = quotaDefinition.Id,
                                Id = Cassandra.TimeUuid.NewId(),
                                StatusCode = context.Response.StatusCode,
                                InsertTime = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds,
                                TransactionTime = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds,
                                QuotaKeySourceValue = quotaKeySourceValue
                            }, _quotaManagementClient));
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

    public class QuotaManagementJob : Job, IJob
    {
        private readonly IQuotaManagementClient _quotaManagementClient;
        public QuotaManagementJob(object data, IQuotaManagementClient quotaManagementClient) : base(data)
        {
            this._quotaManagementClient = quotaManagementClient;
        }

        public override async Task Do(IWorker worker)
        {
            await Task.Run(() =>
            {
                QuotaTransaction quotaTransaction = (QuotaTransaction)data;

                _quotaManagementClient.AddTransaction(quotaTransaction);

            });
        }
    }


    public class QuotaExceedException : Exception
    {
        public int Code => (int)HttpStatusCode.TooManyRequests;
        public long Limit;
        public long Remaining;
        public long Reset;

        public QuotaExceedException(string message, long limit, long remaining, long reset) : base(message)
        {
            this.Limit = limit;
            this.Remaining = remaining;
            this.Reset = reset;
        }
    }
}

