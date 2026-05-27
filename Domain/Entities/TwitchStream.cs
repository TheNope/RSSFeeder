using System;
using System.Text.Json.Serialization;

namespace Domain.Entities;

public class TwitchStream
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("user_login")]
    public string UserLogin { get; set; }
    [JsonPropertyName("user_name")]
    public string UserName { get; set; }
    [JsonPropertyName("game_name")]
    public string GameName { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("started_at")]
    public DateTime StartedAt { get; set; }
    [JsonPropertyName("thumbnail_url")]
    public string Thumbnail { get; set; }
}
