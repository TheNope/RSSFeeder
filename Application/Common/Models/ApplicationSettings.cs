namespace Application.Common.Models;

public class ApplicationSettings
{
    public string Origins { get; set; }
    public string MinimumLogLevel { get; set; }
    public string HttpConnectionsLogLevel { get; set; }
    public string TwitchToken { get; set; }
    public string TwitchClientId { get; set; }
}