namespace FluffyVoid.Networking.Udp;

/// <summary>
///     Event args used to store UDP message information for a client by Tag
/// </summary>
public class UdpSendByTagRequestEventArgs : EventArgs
{
    /// <summary>
    ///     The message to send to the client
    /// </summary>
    public UdpMessage Message { get; }

    /// <summary>
    ///     Constructor used to initialize the data
    /// </summary>
    /// <param name="name">Name of the client the message is intended for</param>
    /// <param name="message">The message to send to the client</param>
    public UdpSendByTagRequestEventArgs(string name, UdpMessage message)
    {
        Message = message;
    }
}