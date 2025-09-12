namespace FluffyVoid.Networking.Udp;

/// <summary>
///     Defines a contract for any class that can be serialized and deserialized to a udp network stream
/// </summary>
public interface IUdpSerializable
{
    /// <summary>
    ///     Defines functionality for deserializing data from a UDP reader
    /// </summary>
    /// <param name="e">Event arg containing data needed to read from the UDP stream</param>
    void Deserialize(UdpDeserializerEventArgs e);
    /// <summary>
    ///     Defines functionality for serializing data to a UDP writer
    /// </summary>
    /// <param name="e">Event arg containing data needed to write to the UDP stream</param>
    int Serialize(UdpSerializerEventArgs e);
}