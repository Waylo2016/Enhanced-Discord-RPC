namespace EnhancedRpc.Host.Data;

public class ReceiveDataMessage
{
    public string? title { get; set; } = "";
    public string? channel { get; set; } = "";
    public string? channelLink { get; set; } = "";
    public string? channelIcon { get; set; } = "";
    public string? videoLink { get; set; } = "";
    public bool? paused { get; set; }
    public double? position { get; set; }
    public double? duration { get; set; }
    
    public string? status { get; set; } = "";
}