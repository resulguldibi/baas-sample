using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Transactions;
using client.cassandra.core;
using job_dispatcher.src.main.core.dispatcher;
using job_dispatcher.src.main.core.dispatcher.provider;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace baas_sample_api
{
    public class BaasExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BaasExceptionHandlingMiddleware> _logger;
        private readonly IDispatcher _logDispatcher;
        private readonly ILoggingClient _loggingClient;

        public BaasExceptionHandlingMiddleware(RequestDelegate next, ILoggingClient loggingClient, ILogger<BaasExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _logDispatcher = DispatcherProvider.GetDispatcher("baas-exception-handling-midlleware-dispatcher", 5, 10000);
            _logDispatcher.Start();
            _loggingClient = loggingClient;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var watch = new System.Diagnostics.Stopwatch();
            string? transactionId = string.Empty;
            string? tppCode = string.Empty;
            try
            {
                StringValues transactionIdValue;
                if (context.Request.Headers.TryGetValue("x-transaction-id", out transactionIdValue))
                {
                    transactionId = transactionIdValue.FirstOrDefault();
                }


                StringValues tppCodeValue;
                if (context.Request.Headers.TryGetValue("x-tpp-code", out tppCodeValue))
                {
                    tppCode = tppCodeValue.FirstOrDefault();
                }

                watch.Start();

                await _next(context);

                watch.Stop();
            }
            catch (Exception ex)
            {
                watch.Stop();

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

                string queryString = string.Empty;

                if (context.Request.QueryString.HasValue && context.Request.QueryString.Value != null)
                {
                    queryString = (string)context.Request.QueryString.Value;
                }

                string path = string.Empty;

                if (context.Request.Path.HasValue && context.Request.Path.Value != null)
                {
                    path = context.Request.Path.Value;
                }

                #region read request body

                context.Request.EnableBuffering();

                var stream = context.Request.Body;

                string requestBodyText = string.Empty;

                using (var reader = new StreamReader(stream))
                {
                    requestBodyText = await reader.ReadToEndAsync();

                    requestBodyText = Regex.Replace(requestBodyText, @"\t|\n|\r", "");

                    byte[] bodyData = Encoding.UTF8.GetBytes(requestBodyText);

                    context.Request.Body = new MemoryStream(bodyData);
                }

                #endregion

                #region prepare error reponse and log transaction details

                string responseBodyText = System.Text.Json.JsonSerializer.Serialize(new ErrorResponseModel() { Message = ex.Message, Code = "SYSTEM_ERROR", Path = path, Id = Guid.NewGuid().ToString() });

                try
                {
                    await _logDispatcher.AddJob(new LoggingJob(new LogItem()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Source = this.GetType().Name,
                        Controller = controller,
                        Action = action,
                        Method = context.Request.Method,
                        QueryString = queryString,
                        ExecutionTime = watch.ElapsedMilliseconds.ToString(),
                        Request = requestBodyText,
                        Response = responseBodyText,
                        Message = ex.Message,
                        StackTrace = ex.StackTrace ?? string.Empty,
                        TransactionId = transactionId ?? string.Empty,
                        StatusCode = (int)HttpStatusCode.InternalServerError,
                        InsertTime = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds,
                        InsertDate = int.Parse(DateTime.Today.ToString("yyyyMMdd")),
                        TppCode = tppCode ?? string.Empty
                    }, this._loggingClient));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                if (ex is RateLimitExceedException)
                {
                    RateLimitExceedException rex = ((RateLimitExceedException)ex);

                    context.Response.StatusCode = rex.Code;
                    context.Response.Headers.TryAdd("x-rate-limit-limit", rex.Limit.ToString());
                    context.Response.Headers.TryAdd("x-rate-limit-remaining", rex.Remaining.ToString());
                    context.Response.Headers.TryAdd("x-rate-limit-reset", rex.Reset.ToString());
                }
                else if (ex is QuotaExceedException)
                {
                    QuotaExceedException qex = ((QuotaExceedException)ex);

                    context.Response.StatusCode = qex.Code;
                    context.Response.Headers.TryAdd("x-quota-limit-limit", qex.Limit.ToString());
                    context.Response.Headers.TryAdd("x-quota-limit-remaining", qex.Remaining.ToString());
                    context.Response.Headers.TryAdd("x-quota-limit-reset", qex.Reset.ToString());
                }

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(responseBodyText);

                #endregion
            }
        }
    }

    public class ErrorResponseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("path")]
        public string Path { get; set; }
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}

