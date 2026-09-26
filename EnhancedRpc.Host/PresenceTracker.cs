using EnhancedRpc.Host.Data;

namespace EnhancedRpc.Host;

public sealed class PresenceTracker
{
    private ReceiveDataMessage? _previous;
    private DateTime _previousAt;
    
    public bool ShouldUpdate(ReceiveDataMessage current)
    {
        if (_previous is null)
        {
            return true;
        }
        
        if (current.position == 0 && !current.paused!.Value)
        {
            return false;
        }
        
        if (_previous is not null) 
        {
            var elapsed = (DateTime.UtcNow - _previousAt).TotalSeconds;
            var expected = _previous.position.GetValueOrDefault() + elapsed;
            var drift = Math.Abs(current.position.GetValueOrDefault() - expected);
            var scrubbed = drift > 2;
            var raw = current.position.GetValueOrDefault() - expected;
            Program.Log($"raw drift: {raw:F2}");
            Program.Log($"drift: {drift:f2}");
            
            return _previous.title != current.title
                   || _previous.paused != current.paused
                   || scrubbed;
        }
        return false;
    }
    
    public void Update(ReceiveDataMessage current)
    {
        _previous = current;
        _previousAt = DateTime.UtcNow;
    }
}