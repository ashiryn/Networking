using System.Net.Sockets;
using FluffyVoid.Utilities;

namespace FluffyVoid.Networking.Udp;

/// <summary>
///     Utility class used to read and write a udp network stream to a dev friendly packet
/// </summary>
public class UdpMessage : INetworkMessage
{
    /// <summary>
    ///     The buffer mode to assign to the message, helps to prevent trying to read from a write buffer and vice-versa
    /// </summary>
    private readonly MessageBufferMode _bufferMode;
    /// <summary>
    ///     The udp stream created for this buffer based on the buffer mode
    /// </summary>
    private readonly UdpStream _stream;

    /// <summary>
    ///     Size of the network buffer
    /// </summary>
    public static ushort BufferSize { get; set; } = 512;

    /// <summary>
    ///     Reference to the streams byte buffer
    /// </summary>
    public byte[] Buffer => _stream.Buffer;
    /// <summary>
    ///     Id of the client sending the message
    /// </summary>
    public short Id { get; }
    /// <summary>
    ///     The number of bytes in the stream buffer
    /// </summary>
    public int Length => _stream.Length;
    /// <summary>
    ///     The tag assigned to the message
    /// </summary>
    public ushort Tag { get; }

    /// <summary>
    ///     Constructor used to builds a message for use in writing to the network stream
    /// </summary>
    /// <param name="tag">The tag to assign to the message</param>
    public UdpMessage(ushort tag)
    {
        Tag = tag;
        _stream = new UdpWriter(BufferSize);
        _bufferMode = MessageBufferMode.Write;
        GetWriter().WriteShort(Id);
        GetWriter().WriteUshort(Tag);
    }
    /// <summary>
    ///     Constructor used to builds a message for use in writing to the network stream
    /// </summary>
    /// <param name="tag">The tag to assign to the message</param>
    public UdpMessage(Enum tag)
    {
        Tag = tag.ToUshort();
        _stream = new UdpWriter(BufferSize);
        _bufferMode = MessageBufferMode.Write;
        GetWriter().WriteShort(Id);
        GetWriter().WriteUshort(Tag);
    }
    /// <summary>
    ///     Constructor used to build a message for reading from the network stream
    /// </summary>
    /// <param name="udpStream">Reference to the udp result</param>
    public UdpMessage(UdpReceiveResult udpStream)
    {
        _stream = new UdpReader(udpStream.Buffer, udpStream.Buffer.Length);
        _bufferMode = MessageBufferMode.Read;
        Id = GetReader().ReadShort();
        Tag = GetReader().ReadUshort();
    }

    /// <summary>
    ///     Returns the reader assigned to this message for use in reading variables from the network stream
    /// </summary>
    /// <returns>The assigned reader if the message is in Read mode, otherwise an empty reader</returns>
    public UdpReader GetReader()
    {
        if (_stream is UdpReader reader)
        {
            return reader;
        }

        return new UdpReader(_bufferMode);
    }
    /// <summary>
    ///     Returns the writer assigned to this message for use in writing variables to the network stream
    /// </summary>
    /// <returns>The assigned writer if the message is in Write mode, otherwise an empty writer</returns>
    public UdpWriter GetWriter()
    {
        if (_stream is UdpWriter writer)
        {
            return writer;
        }

        return new UdpWriter(_bufferMode);
    }
}