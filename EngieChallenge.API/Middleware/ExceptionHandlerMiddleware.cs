using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using EngieChallenge.CORE.Domain.Exceptions;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred.");

        var statusCode = HttpStatusCode.InternalServerError;
        var response = new ErrorResponse
        {
            Message = "An error occurred while processing your request.",
            Details = exception.Message,
            Timestamp = DateTime.UtcNow
        };

        if (exception is PlannedOutputCalculationException)
        {
            statusCode = HttpStatusCode.BadRequest;
            response.Message = "Error in calculating planned output.";
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
}
