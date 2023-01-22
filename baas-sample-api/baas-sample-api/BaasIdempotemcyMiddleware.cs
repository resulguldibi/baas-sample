using System;
using System.Net;
using System.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using static System.Net.Mime.MediaTypeNames;

namespace baas_sample_api
{
    public class BaasIdempotemcyMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<BaasIdempotemcyMiddleware> _logger;
        private readonly IIdempotemcyClient _idempotemcyClient;

        public BaasIdempotemcyMiddleware(RequestDelegate next, IIdempotemcyClient idempotemcyClient, ILogger<BaasIdempotemcyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _idempotemcyClient = idempotemcyClient;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string? transactionId = string.Empty;
            string randomValue = (new Guid()).ToString();
            if (context.Request.Method.Equals(HttpMethod.Post.Method))
            {
                StringValues transactionIdValue;
                if (context.Request.Headers.TryGetValue("x-transaction-id", out transactionIdValue))
                {
                    transactionId = transactionIdValue.FirstOrDefault();

                    if (transactionId == null)
                    {
                        throw new Exception("x-transaction-id is missing");
                    }

                    if (string.IsNullOrEmpty(transactionId))
                    {
                        throw new Exception("x-transaction-id is null or empty");
                    }

                    bool isIdempotent = false;

                    IdempotentData? idempotentData;

                    string applicationId = string.Empty;

                    //request body hash value might be add to idempotemcy key if needed

                    string idempotemcyKey = $"{applicationId}|{transactionId}";

                    (isIdempotent, idempotentData) = _idempotemcyClient.IsIdempotent(idempotemcyKey, randomValue);

                    if (isIdempotent && idempotentData != null)
                    {
                        context.Response.StatusCode = idempotentData.StatusCode;

                        context.Response.ContentType = idempotentData.ContentType;

                        context.Response.Headers.Add("x-idempotent", "true");

                        await context.Response.WriteAsync(idempotentData.Body);

                        return;
                    }
                }
                else
                {
                    throw new Exception("x-transaction-id is missing");
                }
            }

            var originalBodyStream = context.Response.Body;

            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                await _next(context);

                if (context.Request.Method.Equals(HttpMethod.Post.Method) && (context.Response.StatusCode.Equals((int)HttpStatusCode.Created) || context.Response.StatusCode.Equals((int)HttpStatusCode.OK)))
                {
                    string applicationId = string.Empty;

                    string idempotemcyKey = $"{applicationId}|{transactionId}";

                    context.Response.Body.Seek(0, SeekOrigin.Begin);

                    string responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();

                    string? contentType = context.Response.ContentType;

                    if (string.IsNullOrEmpty(contentType))
                    {
                        contentType = "application/json";
                    }

                    var idempotentData = new IdempotentData()
                    {
                        ContentType = contentType,
                        Body = responseBodyText,
                        StatusCode = context.Response.StatusCode
                    };

                    _idempotemcyClient.MarkAsIdempotent(idempotemcyKey, randomValue, idempotentData, 60 * 60 * 1000);
                }

                context.Response.Body.Seek(0, SeekOrigin.Begin);

                await responseBody.CopyToAsync(originalBodyStream);
            }

            context.Response.Body = originalBodyStream;
        }
    }
}

