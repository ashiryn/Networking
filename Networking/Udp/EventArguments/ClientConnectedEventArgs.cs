namespace FluffyVoid.Networking.Udp;

/// <summary>
///     Event arg for use when a client has connected to an endpoint
/// </summary>
public class ClientConnectedEventArgs : EventArgs
{
    /// <summary>
    ///     The client information to send with the event arg
    /// </summary>
    public ClientConnectionInformation ClientInfo { get; }

    /// <summary>
    ///     Constructor used to initialize the event arg
    /// </summary>
    /// <param name="info">The client information data to store</param>
    public ClientConnectedEventArgs(ClientConnectionInformation info)
    {
        ClientInfo = info;
    }
}