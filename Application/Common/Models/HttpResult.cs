using Newtonsoft.Json;
using System.Net;

namespace Application.Common.Models;

public class HttpResult
{
    public HttpStatusCode Code { get; set; }

    public string Exception { get; set; }

    public string Message { get; set; }

    public object Payload { get; set; }

    public HttpResult(object args)
    {
        switch (args)
        {
            case System.Exception exception:
                this.Code = HttpStatusCode.InternalServerError;
                this.Exception = exception.GetType().Name;
                this.Message = exception.Message;
                break;
            default:
                this.Code = HttpStatusCode.OK;
                this.Message = "Request proceeded successfully.";
                this.Payload = args;
                break;
        }
    }

    public string CreateJson() => JsonConvert.SerializeObject((object) this);
}