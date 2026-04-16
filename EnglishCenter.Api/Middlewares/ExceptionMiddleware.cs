using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Models.Response;
using System.Net;
using System.Text.Json;

namespace EnglishCenter.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred. Path: {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        ApiResponse<object> apiResponse;

        switch (exception)
        {
            case NotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                apiResponse = ApiResponse<object>.FailResponse(exception.Message);
                break;

            case BusinessException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                apiResponse = ApiResponse<object>.FailResponse(exception.Message);
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                apiResponse = ApiResponse<object>.FailResponse("Internal server error");
                break;
        }

        var json = JsonSerializer.Serialize(apiResponse);
        await response.WriteAsync(json);
    }
}