using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Entities.Twitch;
using Rest.Interfaces;

namespace Rest;

public class TwitchClient :  ITwitchClient
{
    private static readonly HttpClient HttpClient = new();
    private string _clientId;
    private string _clientSecret;
    private readonly JsonSerializerOptions _querySerializerOptions;
    private readonly JsonSerializerOptions _resultSerializerOptions;

    public TwitchClient(string uri, string clientId, string clientSecret)
    {
        HttpClient.BaseAddress = new(uri);

        this._clientId = clientId;
        this._clientSecret = clientSecret;

        _querySerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        _resultSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
    }

    public async Task UpdateToken()
    {
        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("client_id", _clientId), 
            new KeyValuePair<string, string>("client_secret", _clientSecret),
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        ]);
        
        var response = await HttpClient.PostAsync($"https://id.twitch.tv/oauth2/token", content);
        response.EnsureSuccessStatusCode();
        
        var accessToken = JsonDocument.Parse(await response.Content.ReadAsStringAsync())
            .RootElement
            .Deserialize<AccessToken>(_resultSerializerOptions);
        
        HttpClient.DefaultRequestHeaders.Accept.Clear();
        HttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken.Token}");
        HttpClient.DefaultRequestHeaders.Add("Client-Id", _clientId);
    }

    public async Task<List<Stream>> GetStreams(string channel)
    {
        var response = await HttpClient.GetAsync($"/helix/streams?user_login={channel}");
        response.EnsureSuccessStatusCode();
        
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync())
            .RootElement.GetProperty("data")
            .Deserialize<List<Stream>>(_resultSerializerOptions);
    }
}
