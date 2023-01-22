using System;
using job_dispatcher.src.main.core.dispatcher;
using job_dispatcher.src.main.core.dispatcher.provider;

namespace baas_sample_api
{
    public class BaasRateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BaasRateLimitingMiddleware> _logger;
        private readonly IDispatcher _logDispatcher;


        public BaasRateLimitingMiddleware(RequestDelegate next, ILogger<BaasRateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _logDispatcher = DispatcherProvider.GetDispatcher("baas-rate-limiting-midlleware-dispatcher", 5, 10000);
            _logDispatcher.Start();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //check rate limiting values sync
            await _next(context);
            //update rate limiting values async or sync
            //update rate limiting values async with dispatcher
        }
    }
}

