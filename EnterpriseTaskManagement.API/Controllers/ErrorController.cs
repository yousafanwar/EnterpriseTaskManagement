using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseTaskManagement.API.Controllers;

[ApiController]
public class ErrorController : ControllerBase
{
    [Route("/error")]
    public IActionResult Error()
    {
        var exception = HttpContext.Features
            .Get<IExceptionHandlerFeature>()?
            .Error;

        if (exception is KeyNotFoundException)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Resource not found.",
                detail: exception.Message
            );
        }

        if (exception is UnauthorizedAccessException)
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden.",
                detail: exception.Message
            );
        }

        if (exception is InvalidOperationException)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid operation.",
                detail: exception.Message
            );
        }

        if (exception is ArgumentException)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid argument.",
                detail: exception.Message
            );
        }

        return Problem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "An unexpected error occurred."
        );
    }
}