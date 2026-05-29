using System.Text.Json.Serialization;

namespace Domain.Entities.Twitch;

public class AccessToken
{
    [JsonPropertyName("access_token")]
    public string Token { get; set; }
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }
}
