using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rest.Interfaces;

namespace Rest;

public static class DependencyInjection
{
    public static IServiceCollection AddRestInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var twitchToken = configuration.GetSection("TwitchToken").Value;
        var twitchClientId = configuration.GetSection("TwitchClientId").Value;
        var twitchClient = new TwitchClient("https://api.twitch.tv", twitchToken, twitchClientId);

        services.AddSingleton<ITwitchClient>(twitchClient);
        
        return services;
    }
}
