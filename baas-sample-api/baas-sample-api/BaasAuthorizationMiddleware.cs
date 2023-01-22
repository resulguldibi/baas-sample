using System;

namespace baas_sample_api
{
    public class BaasAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BaasAuthorizationMiddleware> _logger;

        public BaasAuthorizationMiddleware(RequestDelegate next, ILogger<BaasAuthorizationMiddleware> logger)
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

