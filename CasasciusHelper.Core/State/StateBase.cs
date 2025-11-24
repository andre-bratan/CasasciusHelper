using System.Text.Json.Serialization;

namespace CasasciusHelper.Core.State;

/// <summary>
/// Provides a mechanism to notify Blazor of internal application state changes
/// </summary>
public class StateBase
{
    private event EventHandler? OnStateChanged;

    private int onStateChangedSubscribersCount;

    [JsonIgnore]
    public int OnStateChangedSubscribersCount => onStateChangedSubscribersCount;

    [JsonIgnore]
    public bool ShouldUpdateData => onStateChangedSubscribersCount > 0;

    public void RegisterStateChangedCallback(EventHandler callback)
    {
        OnStateChanged += callback;
        Interlocked.Increment(ref onStateChangedSubscribersCount);
    }

    public void UnregisterStateChangedCallback(EventHandler callback)
    {
        OnStateChanged -= callback;
        Interlocked.Decrement(ref onStateChangedSubscribersCount);
    }

    public virtual void NotifyStateChanged()
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
}
