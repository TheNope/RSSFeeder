using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class ErrorController : ApiController
{
    [HttpGet]
    public async Task<ActionResult> Error()
    {
        return Problem(
            title: "An unexpected error occurred.",
            statusCode: StatusCodes.Status500InternalServerError);
    }
}
