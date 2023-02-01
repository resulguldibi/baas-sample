namespace baas_sample_api
{
    public static class MiddlewareExtension
    {
        public static IApplicationBuilder UseBaasAuthentication(this IApplicationBuilder applicationBuilder)
        {
            return applicationBuilder.UseMiddleware<BaasAuthenticationMiddleware>();
        }

        public static IApplicationBuilder UseBaasAuthorization(this IApplicationBuilder applicationBuilder)
        {
            return applicationBuilder.UseMiddleware<BaasAuthorizationMiddleware>();
        }

        public static IApplicationBuilder UseBaasDataIntegrity(this IApplicationBuilder applicationBuilder)
        {
            return applicationBuilder.UseMiddleware<BaasDataIntegrityMiddleware>();
        }

        public static IApplicationBuilder UseBaasExceptionHandling(this IApplicationBuilder applicationBuilder)
        {
            return applicationBuilder.UseMiddleware<BaasExceptionHandlingMiddleware>();
        }

        public static IApplicationBuilder UseBaasIdempotemcy(this IApplicationBuilder applicationBuilder)
        {
            return applicationBuilder.UseMiddleware<BaasIdempotemcyMiddleware>();
        }

        public static IApplicationBuilder UseBaasLogging(this IApplicationBuilder applicationBuilder)
        {
            return applicationBuilder.UseMiddleware<BaasLoggingMiddleware>();
        }

        public static IApplicationBuilder UseBaasRatelimiting(this IApplicationBuilder applicationBuilder)
        {
            return applicationBuilder.UseMiddleware<BaasRateLimitingMiddleware>();
        }

        public static IApplicationBuilder UseBaasQuotaManagement(this IApplicationBuilder applicationBuilder)
        {
            return applicationBuilder.UseMiddleware<BaasQuotaManagementMiddleware>();
        }


        public static IApplicationBuilder UseBaasConsent(this IApplicationBuilder applicationBuilder)
        {
            return applicationBuilder.UseMiddleware<BaasConsentMiddleware>();
        }

    }
}

