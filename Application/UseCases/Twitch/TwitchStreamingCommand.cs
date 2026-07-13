using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models;
using CQRS.Interfaces;
using Domain.Entities;
using Domain.Entities.Twitch;
using Microsoft.Extensions.Logging;
using Rest.Interfaces;
using ServiceStack.Text;

namespace Application.UseCases.Twitch
{
    public class TwitchStreamingCommand : IRequest<HttpResult>
    {
        [DefaultValue("channel")]
        public string Channel { get; set; }
    }
    public class TwitchStreamingCommandHandler : IRequestHandler<TwitchStreamingCommand, HttpResult>
    {
        private readonly ILogger<TwitchStreamingCommandHandler> _logger;
        private readonly ITwitchClient _twitchClient;

        public TwitchStreamingCommandHandler(ILogger<TwitchStreamingCommandHandler> logger,  ITwitchClient twitchClient)
        {
            _logger = logger;
            _twitchClient = twitchClient;
        }

        public async Task<HttpResult> Handle(TwitchStreamingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                List<Stream> streams;
                
                _logger.LogInformation($"Twitch streaming request started for channel {request.Channel}");
                
                try
                {
                    streams = await _twitchClient.GetStreams(request.Channel);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning("Twitch request failed, trying to regenerate twitch token...");
                    var token = await _twitchClient.UpdateToken();

                    if (token.Token.IsNullOrEmpty())
                        throw new HttpRequestException("Twitch token generation failed");
                            
                    _logger.LogInformation("Twitch token generated successfully.");
                    
                    streams = await _twitchClient.GetStreams(request.Channel);
                }
                
                var feed = new Feed
                {
                    Title = request.Channel,
                    Description = $"Streams for {request.Channel}",
                    Language = "en",
                    Items = []
                };

                foreach (var stream in streams)
                {
                    feed.Items.Add(new FeedItem
                    {
                        Guid = Guid.NewGuid(),
                        Title = stream.Title,
                        Description = $"{stream.UserName} is streaming {stream.GameName}",
                        Image = stream.Thumbnail.Replace("{height}", "360").Replace("{width}", "640"),
                        PubDate = stream.StartedAt,
                        Link = "https://twitch.tv/" + stream.UserLogin
                    });
                }
                
                _logger.LogInformation($"Twitch streaming request completed successfully for channel {request.Channel}");

                return new HttpResult(feed.Serialize());
            }
            catch (Exception ex)
            {
                return new HttpResult(ex);
            }
        }
    }
}
