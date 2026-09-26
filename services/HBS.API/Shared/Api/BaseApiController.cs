using System.Diagnostics;
using HBS.API.Shared.enums;
using HBS.API.Shared.Result;
using Microsoft.AspNetCore.Mvc;
using IResult = HBS.API.Shared.Result.IResult;

namespace HBS.API.Shared.Api;

[ApiController]
[Route("api/v1/")]
public abstract class BaseApiController : ControllerBase
{
    // Generic overload for Result<T>
    protected ActionResult HandleResult<T>(IResult<T> result)
    {
        return BuildResponse(result, result.IsSuccess ? result.Value : null);
    }

    // Non-generic overload to handle base Result instances
    protected ActionResult HandleResult(IResult result)
    {
        return BuildResponse(result, null);
    }

    private ActionResult BuildResponse(IResult result, object? value)
    {
        var response = new Dictionary<string, object?>
        {
            { "isSuccess", result.IsSuccess },
            { "timestamp", DateTime.UtcNow },
            { "statusCode", result.StatusCode },
            { "traceId", Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        };

        if (result.IsSuccess && value != null)
        {
            response.Add("value", value);
        }
        else if (result.IsFailure)
        {
            var firstError = result.Error.FirstOrDefault();

            if (firstError != null)
            {
                var errorResponse = new Dictionary<string, object?>
                {
                    { "title", firstError.Title },
                    { "type", GetTypeUri(firstError.Type) },
                    { "detail", firstError.Description },
                    { "instance", HttpContext.Request.Path },
                    { "code", firstError.Code }
                };

                if (firstError.Extensions is not null)
                {
                    errorResponse.Add(
                        "details",
                        firstError.Extensions
                    );
                }

                response.Add("errors", errorResponse);
            }
        }

        return StatusCode(result.StatusCode, response);
    }

    private static string GetTypeUri(ErrorKind? type) => type switch
    {
        ErrorKind.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        ErrorKind.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ErrorKind.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        ErrorKind.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
    };
}