using System.Web;
using DiscordRPC;
using DiscordRPC.Logging;
using EnhancedRpc.Host.Data;

namespace EnhancedRpc.Discord;

public class Rpc
{
    private const string ClientId = "1553126817907347478";
    private static readonly HttpClient _httpClient = new();

    private readonly Dictionary<string, string> _thumbnailCache = new();

    public async Task SetPresence(ReceiveDataMessage data, DiscordRpcClient client)
    {
        var presence = new RichPresence();

        if (!data.status!.Equals("idle"))
        {
            presence
                .WithDetails(data.title)
                .WithDetailsUrl(data.videoLink)
                .WithState(data.channel)
                .WithStateUrl(data.channelLink)
                .WithAssets(new Assets
                {
                    LargeImageKey = await GetLargeImageKeyAsync(data.videoLink),
                    SmallImageKey = data.channelIcon
                })
                .WithType(ActivityType.Listening);
        }
        else
        {
            presence
                .WithDetails("Idle")
                .WithType(ActivityType.Listening);
        }


        var position = data.position.GetValueOrDefault();
        var start = DateTime.UtcNow.AddSeconds(-position);

        if (data.duration.HasValue && !data.paused!.Value)
        {
            presence.WithTimestamps(new Timestamps
            {
                Start = start,
                End = start.AddSeconds(data.duration.Value)
            });
        }

        //Set the rich presence
        client.SetPresence(presence);
    }

    private static readonly string[] ThumbnailQualities =
    [
        "maxresdefault",
        "sddefault",
        "hqdefault"
    ];

    private async Task<string> GetLargeImageKeyAsync(string videoLink)
    {
        var query = new Uri(videoLink).Query;
        var videoId = HttpUtility.ParseQueryString(query).Get("v");
        if (string.IsNullOrEmpty(videoId))
        {
            return "";
        }

        if (_thumbnailCache.TryGetValue(videoId, out var cached))
        {
            return cached;
        }

        string url = "";
        foreach (var quality in ThumbnailQualities)
        {
            url = $"https://img.youtube.com/vi/{videoId}/{quality}.jpg";

            if (await ExistsAsync(url))
            {
                return url;
            }
        }

        _thumbnailCache[videoId] = url;
        return url;
    }

    private static async Task<bool> ExistsAsync(string url)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, url);
            using var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<DiscordRpcClient> Connect()
    {
        var client = new DiscordRpcClient(ClientId)
        {
            Logger = new ConsoleLogger(LogLevel.Info, true)
        };

        client.Initialize();

        return client;
    }

    public void Disconnect(DiscordRpcClient client)
    {
        client.Deinitialize();
    }
}