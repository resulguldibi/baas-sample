using System;

namespace baas_sample_api
{
    public class BaasConsentMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BaasConsentMiddleware> _logger;

        public BaasConsentMiddleware(RequestDelegate next, ILogger<BaasConsentMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            await _next(context);
        }
    }
}

