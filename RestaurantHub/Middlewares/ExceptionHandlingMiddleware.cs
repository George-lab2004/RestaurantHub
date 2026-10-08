namespace RestaurantHub.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;                        // tells us Development vs Production

        public ExceptionHandlingMiddleware(RequestDelegate next,
                                           ILogger<ExceptionHandlingMiddleware> logger,
                                           IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);                                  // run the rest of the pipeline
            }
            catch (Exception ex)                                       // anything unhandled below us lands here
            {
                _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                    context.Request.Method, context.Request.Path);     // full details go to the LOG, never to the user

                if (_env.IsDevelopment()) throw;                       // developers want the detailed error page
                if (context.Response.HasStarted) throw;                // too late: part of the response is already sent

                context.Response.Redirect("/Home/Error");              // users get the friendly page
            }
        }
    }
}