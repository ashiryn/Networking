namespace FluffyVoid.Networking.Udp;

/// <summary>
///     Class used to track and manage a heartbeat with another client/server
/// </summary>
public class HeartBeat
{
    /// <summary>
    ///     Amount of time the ping window will stay open
    /// </summary>
    private readonly float _pingWindow;
    /// <summary>
    ///     Amount of time the pong window will stay open
    /// </summary>
    private readonly float _pongWindow;
    /// <summary>
    ///     Amount of time that has currently elapsed in the current window
    /// </summary>
    private float _elapsed;
    /// <summary>
    ///     Whether the heartbeat is currently in the Ping or the Pong window
    /// </summary>
    private bool _isPingWindowOpen;
    /// <summary>
    ///     Event to notify that the window for waiting on a Ping message has closed
    /// </summary>
    public event Func<Task>? PingWindowEnded;
    /// <summary>
    ///     Event to notify that the window for waiting on a Pong message has closed
    /// </summary>
    public event Func<Task>? PongWindowEnded;

    /// <summary>
    ///     Constructor used to initialize the Ping and Pong windows
    /// </summary>
    /// <param name="pongInterval">Amount of time the pong window will stay open</param>
    /// <param name="pingInterval">Amount of time the ping window will stay open</param>
    public HeartBeat(float pongInterval, float pingInterval)
    {
        _elapsed = _pongWindow = pongInterval;
        _pingWindow = pingInterval;
        _isPingWindowOpen = false;
    }

    /// <summary>
    ///     Update method that calculates whether a window has changed or ended
    /// </summary>
    /// <param name="dt">The amount of time since the last update call</param>
    public async Task Update(float dt)
    {
        _elapsed -= dt;
        if (_elapsed > 0.0f)
        {
            return;
        }

        if (!_isPingWindowOpen)
        {
            _isPingWindowOpen = true;
            _elapsed = _pingWindow;
            if (PongWindowEnded != null)
            {
                await PongWindowEnded.Invoke();
            }
        }
        else
        {
            _isPingWindowOpen = false;
            _elapsed = _pongWindow;
            if (PingWindowEnded != null)
            {
                await PingWindowEnded.Invoke();
            }
        }
    }
}