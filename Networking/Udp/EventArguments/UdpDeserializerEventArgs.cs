namespace FluffyVoid.Networking.Udp;

/// <summary>
///     Event arg used for deserializing custom classes from a UDP network stream
/// </summary>
public class UdpDeserializerEventArgs : EventArgs
{
    /// <summary>
    ///     Reference to the UdpReader assigned to the network stream
    /// </summary>
    public UdpReader Reader { get; }

    /// <summary>
    ///     Constructor used to pass the UdpReader as part of the event arg
    /// </summary>
    /// <param name="reader">The reader to assign to the event arg</param>
    public UdpDeserializerEventArgs(UdpReader reader)
    {
        Reader = reader;
    }
}