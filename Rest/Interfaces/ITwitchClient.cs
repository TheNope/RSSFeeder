using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities.Twitch;

namespace Rest.Interfaces;

public interface ITwitchClient
{
    Task<AccessToken> UpdateToken();
    Task<List<Stream>> GetStreams(string channel);
}
