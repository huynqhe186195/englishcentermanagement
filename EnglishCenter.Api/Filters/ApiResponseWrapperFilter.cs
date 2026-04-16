using EnglishCenter.Application.Common.Models;
using EnglishCenter.Application.Commons.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EnglishCenter.Api.Filters;

public class ApiResponseWrapperFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception != null) return;

        if (context.Result is ObjectResult objectResult)
        {
            if (objectResult.Value is ApiResponse<object> ||
                (objectResult.Value?.GetType().IsGenericType == true &&
                 objectResult.Value.GetType().GetGenericTypeDefinition() == typeof(ApiResponse<>)))
            {
                return;
            }

            var wrapped = new ApiResponse<object>
            {
                Success = true,
                Message = "Success",
                Data = objectResult.Value
            };

            context.Result = new ObjectResult(wrapped)
            {
                StatusCode = objectResult.StatusCode ?? 200
            };
        }
        else if (context.Result is EmptyResult)
        {
            context.Result = new OkObjectResult(new ApiResponse<object>
            {
                Success = true,
                Message = "Success",
                Data = null
            });
        }
    }
}