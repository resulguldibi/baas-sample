using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using static System.Collections.Specialized.BitVector32;

namespace baas_sample_api
{
    public class BaasAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BaasAuthenticationMiddleware> _logger;


        public BaasAuthenticationMiddleware(RequestDelegate next, ILogger<BaasAuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            StringValues values;
            if (!context.Request.Headers.TryGetValue("Authorization", out values))
            {
                throw new Exception("missing authorization header");
            }

            await _next(context);
        }
    }
}

