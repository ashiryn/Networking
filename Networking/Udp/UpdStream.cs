namespace FluffyVoid.Networking.Udp;

/// <summary>
///     Base class for udp streams
/// </summary>
public abstract class UdpStream
{
    /// <summary>
    ///     The byte buffer the stream uses to operate on the network stream
    /// </summary>
    public byte[] Buffer { get; protected init; } = null!;
    /// <summary>
    ///     The length of the byte buffer
    /// </summary>
    public int Length { get; protected set; }
    /// <summary>
    ///     The buffer mode the stream has been set to
    /// </summary>
    protected MessageBufferMode BufferMode { get; init; }
    /// <summary>
    ///     The current position of the pointer in the buffer
    /// </summary>
    protected int Position { get; set; }
}