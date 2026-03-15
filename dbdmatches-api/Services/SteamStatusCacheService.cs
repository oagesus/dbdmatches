using System.Collections.Concurrent;
using DbdMatches.Api.Models;

namespace DbdMatches.Api.Services;

public record SteamStatusEntry(SteamStatus Status, DateTimeOffset UpdatedAt);

public class SteamStatusCacheService
{
    private readonly ConcurrentDictionary<string, SteamStatusEntry> _statusCache = new();

    public void Set(string steamId, SteamStatus status)
    {
        _statusCache[steamId] = new SteamStatusEntry(status, DateTimeOffset.UtcNow);
    }

    public SteamStatusEntry Get(string steamId)
    {
        return _statusCache.TryGetValue(steamId, out var entry)
            ? entry
            : new SteamStatusEntry(SteamStatus.Offline, DateTimeOffset.UtcNow);
    }

    public void SetBulk(Dictionary<string, SteamStatus> statuses)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var (steamId, status) in statuses)
        {
            _statusCache[steamId] = new SteamStatusEntry(status, now);
        }
    }

    public Dictionary<string, SteamStatusEntry> GetAll()
    {
        return new Dictionary<string, SteamStatusEntry>(_statusCache);
    }

    public void Remove(string steamId)
    {
        _statusCache.TryRemove(steamId, out _);
    }
}
