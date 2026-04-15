using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QuantityMeasurementApp.Exceptions;

namespace QuantityMeasurementApp.API.Middleware
{
    /// <summary>
    /// Global Exception Handling Middleware for the REST API.
    /// Why: Standardizes error responses across all API endpoints, providing a consistent experience for developers and clients.
    /// What: Catches any unhandled exceptions in the request pipeline and formats them into a clean JSON response.
    /// How: Registered in Program.cs and executes for every HTTP request that reaches the app.
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        /// <summary>
        /// Initializes the middleware with the next delegate in the pipeline and a logger.
        /// </summary>
        /// <param name="next">The next middleware to execute.</param>
        /// <param name="logger">The logger to capture errors for internal monitoring.</param>
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware for each request.
        /// How: Wraps the execution of the rest of the pipeline in a try-catch block.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Logs the stack trace and error message for the development team.
                _logger.LogError(ex, "CRITICAL ERROR: An unhandled exception occurred in the API pipeline.");
                
                // Returns a structured error response to the client.
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Formats and writes the exception details to the HTTP response.
        /// Why: Prevents leaking sensitive server information while still informing the client of the error type.
        /// How: Sets the status code and content type before writing the serialized ErrorDetails object.
        /// </summary>
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = new ErrorDetails();

            // Logic to map specific application exceptions to appropriate HTTP Status Codes.
            switch (exception)
            {
                case QuantityMeasurementException qme:
                    // Why: Business logic errors (e.g., mixing categories) are typically Client Errors.
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = qme.Message;
                    response.Type = "QuantityValidationException";
                    break;

                case DatabaseException dbe:
                    // Why: Database failures (e.g., connection lost) are Server Errors.
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = "A system error occurred while persisting measurement data. Please contact support.";
                    response.Type = "DatabasePersistenceException";
                    break;

                default:
                    // Why: Catch-all for any other unexpected system crashes.
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = $"Internal Error: {exception.Message}";
                    response.Type = "GlobalSystemException";
                    break;
            }

            // Sets the response status code for the ErrorDetails DTO.
            response.StatusCode = context.Response.StatusCode;

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }

    /// <summary>
    /// Structured response object returned to clients when an error occurs.
    /// </summary>
    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public override string ToString() => JsonSerializer.Serialize(this);
    }
}
