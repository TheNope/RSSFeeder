using System.Threading.Tasks;
using Application.UseCases.Twitch;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers
{
    public class TwitchController : ApiController
    {

        [HttpGet]
        [SwaggerOperation("Returns an rss feed for twitch streams of a channel")]
        public async Task<ActionResult<string>> Get([FromQuery] string channel)
        {
            var result = await Mediator.Send(new TwitchStreamingCommand{Channel = channel});
            
            return new ContentResult
            {
                Content = result.Payload.ToString(),
                ContentType = "application/xml; charset=utf-8",
                StatusCode = (int)result.Code
            };
        }
    }
}
