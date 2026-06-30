using CQRS.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Controllers;

[ApiController]
[Route("api/RSSFeeder/[controller]")]
public abstract class ApiController : ControllerBase
{
    private IMediator _mediator;

    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();
}
