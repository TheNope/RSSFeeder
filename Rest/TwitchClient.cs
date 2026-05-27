using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Entities;
using Rest.Interfaces;

namespace Rest;

public class TwitchClient :  ITwitchClient
{
    private static readonly HttpClient HttpClient = new();
    private readonly JsonSerializerOptions _querySerializerOptions;
    private readonly JsonSerializerOptions _resultSerializerOptions;

    public TwitchClient(string uri, string token, string clientId)
    {
        HttpClient.BaseAddress = new(uri);
        HttpClient.DefaultRequestHeaders.Accept.Clear();
        HttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        HttpClient.DefaultRequestHeaders.Add("Client-Id", clientId);

        _querySerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        _resultSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
    }

    public async Task<List<TwitchStream>> GetStreams(string channel)
    {
        var response = await HttpClient.GetAsync($"/helix/streams?user_login={channel}");
        response.EnsureSuccessStatusCode();
        
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync())
            .RootElement.GetProperty("data")
            .Deserialize<List<TwitchStream>>(_resultSerializerOptions);
    }
}
