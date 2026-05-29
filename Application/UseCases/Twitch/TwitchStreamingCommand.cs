using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models;
using Domain.Entities;
using Domain.Entities.Twitch;
using MediatR;
using Microsoft.Extensions.Logging;
using Rest.Interfaces;

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
                
                try
                {
                    streams = await _twitchClient.GetStreams(request.Channel);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning("Twitch request failed, trying to regenerate twitch token...");
                    await _twitchClient.UpdateToken();
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

                return new HttpResult(feed.Serialize());
            }
            catch (Exception ex)
            {
                return new HttpResult(ex);
            }
        }
    }
}
