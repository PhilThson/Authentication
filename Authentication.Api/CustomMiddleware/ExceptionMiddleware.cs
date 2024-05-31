using Authentication.Core.Exceptions;
using System.Text.Json;

namespace Authentication.Api.CustomMiddleware
{
    public class ExceptionMiddleware
	{
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly string _defaultMsg = "Internal server error occured";

        public ExceptionMiddleware(RequestDelegate next,
            ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception e)
            {
                var statusCode = e switch
                {
                    DataValidationException => StatusCodes.Status400BadRequest,
                    UnauthorizedAccessException
                    or AuthenticationException => StatusCodes.Status401Unauthorized,
                    NotFoundException => StatusCodes.Status404NotFound,
                    _ => StatusCodes.Status500InternalServerError
                };

                _logger.LogError(e, "Error occured");

                httpContext.Response.StatusCode = statusCode;
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsync(
                    GetResponseMessage(
                        string.IsNullOrEmpty(e.Message)
                            ? _defaultMsg : e.Message));
            }
        }

        private static string GetResponseMessage(string message) =>
            JsonSerializer.Serialize(new { message = message });
    }
}

