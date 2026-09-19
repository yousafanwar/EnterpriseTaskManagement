using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseTaskManagement.API.Controllers;

[ApiController]
public class ErrorController : ControllerBase
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    [Route("/error")]
    public IActionResult Error()
    {
        var exception = HttpContext.Features
            .Get<IExceptionHandlerFeature>()?
            .Error;

        if (exception is KeyNotFoundException)
        {
            _logger.LogWarning(
                exception,
                "Resource not found."
            );

            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Resource not found.",
                detail: exception.Message
            );
        }

        if (exception is UnauthorizedAccessException)
        {
            _logger.LogWarning(
                exception,
                "Unauthorized access attempt."
            );

            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden.",
                detail: exception.Message
            );
        }

        if (exception is InvalidOperationException)
        {
            _logger.LogWarning(
                exception,
                "Invalid operation."
            );

            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid operation.",
                detail: exception.Message
            );
        }

        if (exception is ArgumentException)
        {
            _logger.LogWarning(
                exception,
                "Invalid argument."
            );

            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid argument.",
                detail: exception.Message
            );
        }

        _logger.LogError(
            exception,
            "Unhandled exception occurred while processing {Method} {Path}.",
            HttpContext.Request.Method,
            HttpContext.Request.Path
        );

        return Problem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "An unexpected error occurred."
        );
    }
}