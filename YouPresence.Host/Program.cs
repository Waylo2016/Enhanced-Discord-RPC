using System.Text;
using System.Text.Json;
using DiscordRPC;
using EnhancedRpc.Discord;
using EnhancedRpc.Host.Data;
using Microsoft.Win32;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace EnhancedRpc.Host;

public static class Program
{
    private static readonly Encoding LogEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
    
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "YouPresence", "host.log");
    
    private const string HostName = "com.youpresence.waylo.tech";
    private const string ExtensionId = "pomoiackcdfakijgecnhpbbnopplbbdi";
    
    private static async Task<int> Main(string[] args)
    {
        if (args.Contains("--register"))
        {
            Register();
            return 0;
        }
        
        if (args.Contains("--unregister"))
        {
            Unregister();
            return 0;
        }
        
        
        var protocolOut = Console.OpenStandardOutput();
        Console.SetOut(TextWriter.Null);
        Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
        Log("host started");
        
        var rpc = new Rpc();
        var tracker = new PresenceTracker();
        
        DiscordRpcClient client = null!;
        ReceiveDataMessage? lastSent = null;
        ReceiveDataMessage? previous = null;
        Stream input = null!;
        
        
        try
        {
             input = Console.OpenStandardInput();
            
            Log("connecting to discord");
            
            client = await rpc.Connect();
            Log("discord connect returned");
            
            while (true)
            {
                var header = new byte[4];
                var read = await input.ReadAtLeastAsync(header, 4, throwOnEndOfStream: false);
                if (read < 4)
                {
                    Log($"stdin closed, read {read} bytes");
                    break;
                }

                var length = BitConverter.ToInt32(header);
                var payload = new byte[length];
                await input.ReadExactlyAsync(payload);

                
                var json = Encoding.UTF8.GetString(payload);
                var state = JsonSerializer.Deserialize<ReceiveDataMessage>(json);
                Log($"received: {json}");
                
                if (tracker.ShouldUpdate(state))
                {
                    Log("state changed, updating presence");
                    await rpc.SetPresence(state!, client);
                    tracker.MarkSent(state!);
                }
                else
                {
                    Log("state unchanged, skipping presence update");
                }
                
                tracker.Update(state);
            }
        }
        catch (Exception ex)
        {
            Log($"CRASH: {ex}");
            Log(input.ToString());
        }
        Log("stdin closed by browser");
        rpc.Disconnect(client);
        return 0;
    }
    
    public static void Log(string message)
    {
        File.AppendAllText(LogPath, $"{DateTime.Now:HH:mm:ss} {message}{Environment.NewLine}", LogEncoding);
    }
    private static void Register()
    {
        var exePath = Environment.ProcessPath!;
        var manifestPath = Path.Combine(Path.GetDirectoryName(exePath)!, "native-host-manifest.json");

        var manifest = new
        {
            name = HostName,
            description = "YouPresence native host",
            path = exePath,
            type = "stdio",
            allowed_origins = new[] { $"chrome-extension://{ExtensionId}/" }
        };

        File.WriteAllText(manifestPath,
            JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));

        using var key = Registry.CurrentUser.CreateSubKey(
            $@"Software\Google\Chrome\NativeMessagingHosts\{HostName}");
        key.SetValue(null, manifestPath);
    }
    
    private static void Unregister()
    {
        Registry.CurrentUser.DeleteSubKeyTree(
            $@"Software\Google\Chrome\NativeMessagingHosts\{HostName}",
            throwOnMissingSubKey: false);
    }
}