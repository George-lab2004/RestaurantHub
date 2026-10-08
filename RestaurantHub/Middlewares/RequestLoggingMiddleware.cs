using System.Diagnostics;                                   // Stopwatch

namespace RestaurantHub.Middleware
{
    public class RequestLoggingMiddleware
    {
        // requests we don't want in the log (static files + the SignalR connection)
        private static readonly string[] IgnoredPrefixes =
            { "/css", "/js", "/lib", "/images", "/favicon", "/orderHub", "/_framework" };

        private readonly RequestDelegate _next;                         // the next station
        private readonly ILogger<RequestLoggingMiddleware> _logger;     // .NET's built-in logger

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "/";

            // static file? just pass it on, no logging
            if (IgnoredPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();                       // start the clock (way IN)
            try
            {
                await _next(context);                                   // everything else runs here
            }
            finally                                                     // finally = runs even if something threw
            {
                stopwatch.Stop();                                       // way OUT: the response exists now
                var user = context.User.Identity?.IsAuthenticated == true
                    ? context.User.Identity.Name
                    : "anonymous";

                _logger.LogInformation("{Method} {Path} -> {StatusCode} in {ElapsedMs} ms (user: {User})",
                    context.Request.Method, path, context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds, user);
            }
        }
    }
}