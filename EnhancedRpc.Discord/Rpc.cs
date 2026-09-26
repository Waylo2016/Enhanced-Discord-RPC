using System.Web;
using DiscordRPC;
using DiscordRPC.Logging;
using EnhancedRpc.Host.Data;

namespace EnhancedRpc.Discord;

public class Rpc
{
    private const string ClientId = "1553126817907347478";

    public void SetPresence(ReceiveDataMessage data, DiscordRpcClient client)
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
                    LargeImageKey = GetLargeImageKey(data.videoLink),
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

    private string GetLargeImageKey(string videoLink)
    {
        var query = new Uri(videoLink).Query;
        var videoId = HttpUtility.ParseQueryString(query).Get("v");
        return $"https://img.youtube.com/vi/{videoId}/maxresdefault.jpg";
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