using EnhancedRpc.Host.Data;

namespace EnhancedRpc.Host;

public sealed class PresenceTracker
{
    private ReceiveDataMessage? _previous;
    private DateTime _previousAt;
    
    private ReceiveDataMessage? _lastSent;
    
    public bool ShouldUpdate(ReceiveDataMessage current)
    {
        if (current.position == 0 && current.paused != true)
        {
            return false;
        }

        if (_lastSent is null)
        {
            return true;
        }

        if (_lastSent.title != current.title || _lastSent.paused != current.paused)
        {
            return true;
        }

        return HasScrubbed(current);
    }
    
    public void Update(ReceiveDataMessage current)
    {
        _previous = current;
        _previousAt = DateTime.UtcNow;
    }
    public void MarkSent(ReceiveDataMessage current)
    {
        _lastSent = current;
    }

    
    private bool HasScrubbed(ReceiveDataMessage current)
    {
        if (_previous is null || current.paused == true || _previous.paused == true)
        {
            return false;
        }

        var elapsed = (DateTime.UtcNow - _previousAt).TotalSeconds;
        var expected = _previous.position.GetValueOrDefault() + elapsed;
        var drift = Math.Abs(current.position.GetValueOrDefault() - expected);

        return drift > 2;
    }
}