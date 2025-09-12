namespace FluffyVoid.Networking.Udp;

/// <summary>
///     Event arg for use when receiving a UDP message
/// </summary>
public class UdpMessageEventArgs
    : EventArgs, INetworkMessageEventArgs<UdpMessage>
{
    /// <summary>
    ///     The Udp message to send with the event arg
    /// </summary>
    public UdpMessage Message { get; }
    /// <summary>
    ///     The tag associated with the message
    /// </summary>
    public ushort Tag => Message?.Tag ?? 0;
    /// <summary>
    ///     Constructor used to initialize the event arg
    /// </summary>
    /// <param name="message">The udp message to store</param>
    public UdpMessageEventArgs(UdpMessage message)
    {
        Message = message;
    }
}