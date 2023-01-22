using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Transactions;
using client.cassandra.core;
using job_dispatcher.src.main.core.dispatcher;
using job_dispatcher.src.main.core.dispatcher.provider;
using job_dispatcher.src.main.core.job;
using job_dispatcher.src.main.core.worker;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using static System.Net.Mime.MediaTypeNames;

namespace baas_sample_api
{
    public class BaasLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BaasLoggingMiddleware> _logger;
        private readonly IDispatcher _logDispatcher;
        private readonly ILoggingClient _loggingClient;

        public BaasLoggingMiddleware(RequestDelegate next, ILoggingClient loggingClient, ILogger<BaasLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _logDispatcher = DispatcherProvider.GetDispatcher("baas-logging-midlleware-dispatcher", 10, 10000);
            _logDispatcher.Start();
            _loggingClient = loggingClient;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string? transactionId = string.Empty;
            StringValues transactionIdValue;
            if (context.Request.Headers.TryGetValue("x-transaction-id", out transactionIdValue))
            {
                transactionId = transactionIdValue.FirstOrDefault();
            }

            string? tppCode = string.Empty;
            StringValues tppCodeValue;
            if (context.Request.Headers.TryGetValue("x-tpp-code", out tppCodeValue))
            {
                tppCode = tppCodeValue.FirstOrDefault();
            }

            Microsoft.AspNetCore.Routing.RouteData rd = context.Request.HttpContext.GetRouteData();

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

            var watch = new System.Diagnostics.Stopwatch();


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

            #region read response body and call next middleware
            var originalResponseBodyStream = context.Response.Body;

            string responseBodyText = string.Empty;

            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                try
                {
                    watch.Start();

                    await _next(context);

                    watch.Stop();
                }
                finally
                {
                    context.Response.Body.Seek(0, SeekOrigin.Begin);

                    responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();

                    context.Response.Body.Seek(0, SeekOrigin.Begin);

                    await responseBody.CopyToAsync(originalResponseBodyStream);

                    context.Response.Body = originalResponseBodyStream;
                }
            }
            #endregion

            #region log transaction details

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
                    TransactionId = transactionId ?? string.Empty,
                    StatusCode = context.Response.StatusCode,
                    InsertTime = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds,
                    InsertDate = int.Parse(DateTime.Today.ToString("yyyyMMdd")),
                    TppCode = tppCode ?? string.Empty
                }, this._loggingClient));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            #endregion
        }
    }

    public class LoggingJob : Job, IJob
    {
        private readonly ILoggingClient loggingClient;
        public LoggingJob(object data, ILoggingClient loggingClient) : base(data)
        {
            this.loggingClient = loggingClient;
        }

        public override async Task Do(IWorker worker)
        {
            await Task.Run(() =>
            {
                LogItem logItem = (LogItem)data;
                Console.WriteLine(JsonSerializer.Serialize(logItem));

                this.loggingClient.Log(logItem);
            });
        }
    }

    public class LogItem
    {
        [JsonPropertyName("controller")]
        public string Controller { get; set; }
        [JsonPropertyName("action")]
        public string Action { get; set; }
        [JsonPropertyName("method")]
        public string Method { get; set; }
        [JsonPropertyName("queryString")]
        public string QueryString { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("request")]
        public string Request { get; set; }
        [JsonPropertyName("response")]
        public string Response { get; set; }
        [JsonPropertyName("executionTime")]
        public string ExecutionTime { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("stackTrace")]
        public string StackTrace { get; set; }
        [JsonPropertyName("source")]
        public string Source { get; set; }
        [JsonPropertyName("transactionId")]
        public string TransactionId { get; set; }
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }
        [JsonPropertyName("insert_time")]
        public long InsertTime { get; set; }
        [JsonPropertyName("insert_date")]
        public int InsertDate { get; set; }
        [JsonPropertyName("tppCode")]
        public string TppCode { get; set; }
    }


}

