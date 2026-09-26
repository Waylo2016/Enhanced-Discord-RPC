using System.Web;
using DiscordRPC;
using DiscordRPC.Logging;
using Microsoft.Extensions.Configuration;

namespace EnhancedRpc.Discord;

public class Rpc
{
    private const string clientId = "1553126817907347478";
    private static DiscordRpcClient client;
    private static IConfiguration configuration;

    public void ConnectAndSetPresence()
    {
        client = new DiscordRpcClient(clientId)
        {
            Logger = new ConsoleLogger(LogLevel.Info, true)
        };

        configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("config.json", optional: false)
            .Build();

        client.OnReady += (sender, e) =>
        {

            Console.WriteLine("Connected to discord with user {0}", e.User.Username);
            Console.WriteLine("Avatar: {0}", e.User.GetAvatarURL(User.AvatarFormat.WebP));
        };

        //Connect to the Rpc
        client.Initialize();
        Console.WriteLine(configuration["testData:musicIcon"]);
        Console.WriteLine(configuration["testData:title"]);
        Console.WriteLine(configuration["testData:channel"]);

        var presence = new RichPresence()
            .WithDetails(configuration["testData:title"])
            .WithDetailsUrl(configuration["testData:videoLink"])
            .WithState(configuration["testData:channel"])
            .WithStateUrl(configuration["testData:channelLink"])
            .WithAssets(new Assets
            {
                LargeImageKey = GetLargeImageKey(configuration["testData:videoLink"]!),
                SmallImageKey = configuration["testData:channelIcon"]
            })
            .WithTimestamps(new Timestamps()
            {
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddSeconds(Convert.ToDouble(configuration["testData:duration"]))
            })
            .WithButtons(new Button()
            {
                Label = "Watch on YouTube",
                Url = configuration["testData:videoLink"]
            })
            .WithType(ActivityType.Listening);
        
        //Set the rich presence
        client.SetPresence(presence);

        Console.ReadKey();
        client.Dispose();
    }

    private string GetLargeImageKey(string videoLink)
    {
        var query = new Uri(videoLink).Query;
        var videoId = HttpUtility.ParseQueryString(query).Get("v");
        return $"https://img.youtube.com/vi/{videoId}/maxresdefault.jpg";
    }
}