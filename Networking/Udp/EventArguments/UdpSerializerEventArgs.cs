namespace FluffyVoid.Networking.Udp;

/// <summary>
///     Event arg used for serializing custom classes to a UDP network stream
/// </summary>
public class UdpSerializerEventArgs : EventArgs
{
    /// <summary>
    ///     Reference to the UdpWriter assigned to the network stream
    /// </summary>
    public UdpWriter Writer { get; }

    /// <summary>
    ///     Constructor used to pass the UdpWriter as part of the event arg
    /// </summary>
    /// <param name="writer">The writer to assign to the event arg</param>
    public UdpSerializerEventArgs(UdpWriter writer)
    {
        Writer = writer;
    }
}