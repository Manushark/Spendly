using System.Security.Claims;

namespace Spendly.Api.Security
{
    public static class RateLimitPolicies
    {
        public const string Auth = "auth";
        public const string WriteOperations = "write-operations";
        public const string ImportPreview = "import-preview";
        public const string ImportConfirm = "import-confirm";

        public static string GetPartitionKey(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? context.User.FindFirst("sub")?.Value;

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    return $"user:{userId}";
                }
            }

            var remoteIp = context.Connection.RemoteIpAddress?.ToString();
            return string.IsNullOrWhiteSpace(remoteIp) ? "anonymous" : $"ip:{remoteIp}";
        }
    }
}
