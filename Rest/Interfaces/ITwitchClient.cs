using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Rest.Interfaces;

public interface ITwitchClient
{
    Task<List<TwitchStream>> GetStreams(string channel);
}
