using System.Text.Json;
using DbdMatches.Api.DTOs;

namespace DbdMatches.Api.Services;

public class SteamAuthService(IConfiguration configuration, HttpClient httpClient)
{
    private const string SteamOpenIdUrl = "https://steamcommunity.com/openid/login";
    private const string SteamApiUrl = "https://api.steampowered.com";

    public string GetLoginUrl(string returnUrl)
    {
        var parameters = new Dictionary<string, string>
        {
            ["openid.ns"] = "http://specs.openid.net/auth/2.0",
            ["openid.mode"] = "checkid_setup",
            ["openid.return_to"] = returnUrl,
            ["openid.realm"] = new Uri(returnUrl).GetLeftPart(UriPartial.Authority),
            ["openid.identity"] = "http://specs.openid.net/auth/2.0/identifier_select",
            ["openid.claimed_id"] = "http://specs.openid.net/auth/2.0/identifier_select"
        };

        var queryString = string.Join("&", parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
        return $"{SteamOpenIdUrl}?{queryString}";
    }

    public async Task<string?> ValidateOpenIdResponse(Dictionary<string, string> parameters)
    {
        var validationParams = new Dictionary<string, string>(parameters)
        {
            ["openid.mode"] = "check_authentication"
        };

        var content = new FormUrlEncodedContent(validationParams);
        var response = await httpClient.PostAsync(SteamOpenIdUrl, content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!responseString.Contains("is_valid:true"))
            return null;

        var claimedId = parameters.GetValueOrDefault("openid.claimed_id");
        if (claimedId == null)
            return null;


        var steamId = claimedId.Split('/').Last();
        return steamId;
    }

    public async Task<SteamPlayerSummary?> GetPlayerSummary(string steamId)
    {
        var apiKey = configuration["Steam:ApiKey"]
            ?? throw new InvalidOperationException("Steam API key not configured");

        var url = $"{SteamApiUrl}/ISteamUser/GetPlayerSummaries/v0002/?key={apiKey}&steamids={steamId}";
        var response = await httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var players = doc.RootElement
            .GetProperty("response")
            .GetProperty("players");

        if (players.GetArrayLength() == 0)
            return null;

        var player = players[0];

        return new SteamPlayerSummary(
            SteamId: player.GetProperty("steamid").GetString()!,
            PersonaName: player.GetProperty("personaname").GetString()!,
            AvatarFull: player.TryGetProperty("avatarfull", out var avatar) ? avatar.GetString() : null
        );
    }
}
