namespace FluffyVoid.Networking.Udp;

/// <summary>
///     Event args used to store UDP message information for a client by Name
/// </summary>
public class UdpSendByNameRequestEventArgs : EventArgs
{
    /// <summary>
    ///     The message to send to the client
    /// </summary>
    public UdpMessage Message { get; }
    /// <summary>
    ///     Name of the client the message is intended for
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Constructor used to initialize the data
    /// </summary>
    /// <param name="name">Name of the client the message is intended for</param>
    /// <param name="message">The message to send to the client</param>
    public UdpSendByNameRequestEventArgs(string name, UdpMessage message)
    {
        Name = name;
        Message = message;
    }
}