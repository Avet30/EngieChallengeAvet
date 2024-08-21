//using EngieChallenge.CORE.Services.Exceptions;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using System;
//using System.ComponentModel.DataAnnotations;
//using System.Net;
//using System.Text.Json;
//using System.Threading.Tasks;

//public class ExceptionHandlerMiddleware
//{
//    private readonly RequestDelegate _next;
//    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

//    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
//    {
//        _next = next;
//        _logger = logger;
//    }

//    public async Task Invoke(HttpContext context)
//    {
//        try
//        {
//            var requestContent = "";
//            context.Request.EnableBuffering();
//            var reader = new StreamReader(context.Request.Body);

//            requestContent = await reader.ReadToEndAsync();

//            context.Request.Body.Position = 0;
//            _logger.LogInformation(requestContent);
//            await _next(context);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, ex.Message, ex.StackTrace);
//            await ConvertException(context, ex);
//        }
//    }

//    private Task ConvertException(HttpContext context, Exception exception)
//    {
//        HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError;

//        context.Response.ContentType = "application/json";

//        var result = string.Empty;

//        switch (exception)
//        {
//            case EngieChallenge.CORE.Services.Exceptions.ValidationException validationException:
//                httpStatusCode = HttpStatusCode.BadRequest;
//                result = System.Text.Json.JsonSerializer.Serialize(validationException.ValidationErrors);
//                break;
//            case BadRequestException badRequestException:
//                httpStatusCode = HttpStatusCode.BadRequest;
//                result = badRequestException.Message;
//                break;
//            case NotFoundException:
//                httpStatusCode = HttpStatusCode.NotFound;
//                break;
//            case Exception:
//                httpStatusCode = HttpStatusCode.InternalServerError;
//                break;
//        }

//        context.Response.StatusCode = (int)httpStatusCode;

//        if (result == string.Empty)
//        {
//            result = System.Text.Json.JsonSerializer.Serialize(new { error = exception.Message });
//        }

//        return context.Response.WriteAsync(result);
//    }
//}

