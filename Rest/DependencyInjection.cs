using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rest.Interfaces;

namespace Rest;

public static class DependencyInjection
{
    public static IServiceCollection AddRestInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var twitchClientId = configuration.GetSection("TwitchClientId").Value;
        var twitchClientSecret = configuration.GetSection("TwitchClientSecret").Value;
        
        var twitchClient = new TwitchClient("https://api.twitch.tv", twitchClientId, twitchClientSecret);

        services.AddSingleton<ITwitchClient>(twitchClient);
        
        return services;
    }
}
