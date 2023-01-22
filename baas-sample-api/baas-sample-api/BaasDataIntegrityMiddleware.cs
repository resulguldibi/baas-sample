using System;

namespace baas_sample_api
{
    public class BaasDataIntegrityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BaasDataIntegrityMiddleware> _logger;

        public BaasDataIntegrityMiddleware(RequestDelegate next, ILogger<BaasDataIntegrityMiddleware> logger)
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

