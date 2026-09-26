using EnhancedRpc.Discord;

namespace EnhancedRpc.Host;

public static class Program
{
    // probably gonna become private static async Task<int> Main(string[] args) in the future when I get the pipes working
    public static void Main(string[] args)
    {
        Rpc rpc = new Rpc();
        rpc.ConnectAndSetPresence();
    }
}