namespace FluffyVoid.Networking.Udp;

/// <summary>
///     Event arg for use when a client has disconnected from an endpoint
/// </summary>
public class ClientDisconnectedEventArgs : EventArgs
{
    /// <summary>
    ///     The client information to send with the event arg
    /// </summary>
    public ClientConnectionInformation ClientInfo { get; }
    /// <summary>
    ///     Whether the disconnect was initiated by the connected client or not
    /// </summary>
    public bool IsLocalDisconnect { get; }

    /// <summary>
    ///     Constructor used to initialize the event arg
    /// </summary>
    /// <param name="isLocalDisconnect">Whether the disconnect was initiated by the connected client or not</param>
    /// <param name="info">The client information data to store</param>
    public ClientDisconnectedEventArgs(bool isLocalDisconnect,
                                       ClientConnectionInformation info)
    {
        IsLocalDisconnect = isLocalDisconnect;
        ClientInfo = info;
    }
}