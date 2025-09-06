using System.Text.Json;

namespace UMS.API.GlobalAcceptionHandler
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var statusCode = ex switch
            {
                ArgumentNullException => StatusCodes.Status400BadRequest,  // Bad Request
                KeyNotFoundException => StatusCodes.Status404NotFound,    // Not Found
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,// Unauthorized
                InvalidOperationException => StatusCodes.Status409Conflict,    // Conflict
                _ => StatusCodes.Status500InternalServerError // Internal Server Error
            };

            var problem = new
            {
                status = statusCode,
                title = ex.Message,
                detail = ex.InnerException?.Message
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
