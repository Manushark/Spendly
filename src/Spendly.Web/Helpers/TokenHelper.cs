namespace Spendly.Web.Helpers
{
    /// <summary>
    /// Centralized helper for JWT token retrieval.
    /// Tries Session first (fast, in-memory), then falls back to a persistent HTTP-only cookie.
    /// This ensures the token survives Azure App Service recycles.
    /// </summary>
    public static class TokenHelper
    {
        public const string SessionKey = "token";
        public const string CookieName = "SpendlyAuth";

        /// <summary>
        /// Retrieves the JWT token from Session or Cookie.
        /// If found only in Cookie, re-hydrates the Session for performance.
        /// </summary>
        public static string? GetToken(HttpContext? context)
        {
            if (context is null) return null;

            // 1. Try session first (fastest)
            var token = context.Session.GetString(SessionKey);

            if (!string.IsNullOrEmpty(token))
                return token;

            // 2. Fallback: read from persistent cookie
            token = context.Request.Cookies[CookieName];

            if (!string.IsNullOrEmpty(token))
            {
                // Re-hydrate session so subsequent calls in this request are fast
                context.Session.SetString(SessionKey, token);
            }

            return token;
        }

        /// <summary>
        /// Stores the JWT token in both Session and a persistent HTTP-only cookie.
        /// </summary>
        public static void SetToken(HttpContext context, string token)
        {
            // Store in session (fast access during this server lifetime)
            context.Session.SetString(SessionKey, token);

            // Store in persistent cookie (survives app recycles)
            context.Response.Cookies.Append(CookieName, token, new CookieOptions
            {
                HttpOnly = true,       // Not accessible via JavaScript (XSS protection)
                Secure = true,         // Only sent over HTTPS
                SameSite = SameSiteMode.Strict,
                MaxAge = TimeSpan.FromHours(2),  // Matches JWT expiration
                Path = "/"
            });
        }

        /// <summary>
        /// Clears the JWT token from both Session and Cookie.
        /// </summary>
        public static void ClearToken(HttpContext context)
        {
            context.Session.Remove(SessionKey);
            context.Session.Remove("userEmail");
            context.Response.Cookies.Delete(CookieName);
        }
    }
}
