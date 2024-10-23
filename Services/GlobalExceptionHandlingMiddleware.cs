
using Reddit.Models;

namespace Reddit.Services
{
    public class GlobalExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (Exception e)
            {
                _logger.LogError($"an exception has been thrown with the message: {e.Message}");
                string message = "Unexpected error occured on the server";
                int statusCode = StatusCodes.Status500InternalServerError;
                ErrorDetails errorDetails = new ErrorDetails()
                {
                    Message = message,
                    StatusCode = statusCode,
                };

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(errorDetails);
            };
        }
    }
}
