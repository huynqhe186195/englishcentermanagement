using System.Net;
using System.Text.Json;
using EnglishCenter.Application.Common.Exceptions;
using EnglishCenter.Application.Commons.Models.Response;

namespace EnglishCenter.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var apiResponse = new ApiResponse<string>();

        switch (exception)
        {
            case NotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                apiResponse = ApiResponse<string>.FailResponse(exception.Message);
                break;

            case BusinessException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                apiResponse = ApiResponse<string>.FailResponse(exception.Message);
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                apiResponse = ApiResponse<string>.FailResponse("Internal server error");
                break;
        }

        var json = JsonSerializer.Serialize(apiResponse);

        await response.WriteAsync(json);
    }
}