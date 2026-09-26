using System.Text;
using DiscordRPC;
using EnhancedRpc.Discord;
using EnhancedRpc.Host.Data;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace EnhancedRpc.Host;

public static class Program
{
    
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "YouPresence", "host.log");
    
    private static async Task<int> Main(string[] args)
    {
        var protocolOut = Console.OpenStandardOutput();
        Console.SetOut(TextWriter.Null);
        Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
        Log("host started");
        
        DiscordRpcClient client = null!;
        var rpc = new Rpc();
        ReceiveDataMessage? lastSent = null;
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
                
                if (HasChanged(state!, lastSent))
                {
                    Log("state changed, updating presence");
                    rpc.SetPresence(state!, client);
                    lastSent = state;
                }
                else
                {
                    Log("state unchanged, skipping presence update");
                }
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
    
    private static readonly Encoding LogEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
    private static void Log(string message)
    {
        File.AppendAllText(LogPath, $"{DateTime.Now:HH:mm:ss} {message}{Environment.NewLine}", LogEncoding);
    }
    
    private static bool HasChanged(ReceiveDataMessage current, ReceiveDataMessage last)
    {
        
        if (last is null)
        {
            return true;
        }
        
        if (current.position == 0 && !current.paused!.Value)
        {
            return false;
        }
        
        return last.title != current.title
               || last.paused != current.paused;
    }
}