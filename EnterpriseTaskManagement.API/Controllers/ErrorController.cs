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

        if (exception is InvalidOperationException)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }

        return StatusCode(500, new
        {
            message = "An unexpected error occurred."
        });
    }
}