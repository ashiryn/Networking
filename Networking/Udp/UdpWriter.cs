using System.Text;
using FluffyVoid.Logging;

namespace FluffyVoid.Networking.Udp;

/// <summary>
///     Udp writer class used to read values directly from a network byte stream
/// </summary>
public class UdpWriter : UdpStream
{
    /// <summary>
    ///     Maximum size of the message buffer
    /// </summary>
    private readonly ushort _maximumMessageBufferSize;

    /// <summary>
    ///     Default constructor for initializing the udp writer
    /// </summary>
    /// <param name="bufferSize">The max size of the network stream</param>
    public UdpWriter(ushort bufferSize)
    {
        _maximumMessageBufferSize = bufferSize;
        Position = Length = 0;
        Buffer = new byte[_maximumMessageBufferSize];
        BufferMode = MessageBufferMode.Write;
    }
    /// <summary>
    ///     Constructor used to initialize an empty writer
    /// </summary>
    /// <param name="bufferMode"></param>
    public UdpWriter(MessageBufferMode bufferMode = MessageBufferMode.Write)
    {
        _maximumMessageBufferSize = 0;
        Position = Length = 0;
        Buffer = [];
        BufferMode = bufferMode;
    }
    /// <summary>
    ///     Writes a custom serializable class to the network stream
    /// </summary>
    /// <param name="value">The class to write</param>
    public int Write<T>(T value)
        where T : IUdpSerializable
    {
        return value.Serialize(new UdpSerializerEventArgs(this));
    }
    /// <summary>
    ///     Writes a single bool to the network stream
    /// </summary>
    /// <param name="value">The bool value to write</param>
    public int WriteBool(bool value)
    {
        return WriteToBuffer(BitConverter.GetBytes(value));
    }

    /// <summary>
    ///     Writes a single byte to the network stream
    /// </summary>
    /// <param name="value">The byte value to write</param>
    public int WriteByte(byte value)
    {
        return WriteToBuffer([value]);
    }
    /// <summary>
    ///     Writes a single char to the network stream
    /// </summary>
    /// <param name="value">The char value to write</param>
    public int WriteChar(char value)
    {
        return WriteToBuffer(BitConverter.GetBytes(value));
    }
    /// <summary>
    ///     Writes a single double to the network stream
    /// </summary>
    /// <param name="value">The double value to write</param>
    public int WriteDouble(double value)
    {
        return WriteToBuffer(BitConverter.GetBytes(value));
    }
    /// <summary>
    ///     Writes a single float to the network stream
    /// </summary>
    /// <param name="value">The float value to write</param>
    public int WriteFloat(float value)
    {
        return WriteToBuffer(BitConverter.GetBytes(value));
    }
    /// <summary>
    ///     Writes a single int to the network stream
    /// </summary>
    /// <param name="value">The int value to write</param>
    public int WriteInt(int value)
    {
        return WriteToBuffer(BitConverter.GetBytes(value));
    }
    /// <summary>
    ///     Writes a single long to the network stream
    /// </summary>
    /// <param name="value">The long value to write</param>
    public int WriteLong(long value)
    {
        return WriteToBuffer(BitConverter.GetBytes(value));
    }
    /// <summary>
    ///     writes a raw byte buffer at the desired location within the stream
    /// </summary>
    /// <param name="itemBuffer">The byte buffer for the requested variable to be written to the stream</param>
    /// <param name="itemPosition">The starting position to write the byte buffer</param>
    /// <returns>The number of bytes successfully written to the stream</returns>
    public int WriteRaw(byte[] itemBuffer, int itemPosition)
    {
        if (BufferMode != MessageBufferMode.Write)
        {
            LogManager
                .LogError("Attempting to write to a buffer not assigned for writing",
                          nameof(UdpWriter));

            return -1;
        }

        if (itemPosition + itemBuffer.Length < _maximumMessageBufferSize)
        {
            if (BitConverter.IsLittleEndian && itemBuffer.Length > 1)
            {
                itemBuffer = itemBuffer.Reverse().ToArray();
            }

            itemBuffer.CopyTo(Buffer, itemPosition);
            if (itemPosition + itemBuffer.Length > Length)
            {
                Length = itemPosition + itemBuffer.Length;
            }

            return itemBuffer.Length;
        }

        LogManager
            .LogError($"Attempting to write {Length + itemBuffer.Length - _maximumMessageBufferSize} bytes past the end of the buffer",
                      nameof(UdpWriter));

        return -1;
    }
    /// <summary>
    ///     Writes a single short to the network stream
    /// </summary>
    /// <param name="value">The short value to write</param>
    public int WriteShort(short value)
    {
        return WriteToBuffer(BitConverter.GetBytes(value));
    }
    /// <summary>
    ///     Writes a string to the network stream
    /// </summary>
    /// <param name="value">The string value to write</param>
    public int WriteString(string value)
    {
        int count = WriteToBuffer(BitConverter.GetBytes((ushort)value.Length));
        count += WriteToBuffer(Encoding.ASCII.GetBytes(value));
        return count;
    }
    /// <summary>
    ///     Writes a single uint to the network stream
    /// </summary>
    /// <param name="value">The uint value to write</param>
    public int WriteUint(uint value)
    {
        return WriteToBuffer(BitConverter.GetBytes(value));
    }
    /// <summary>
    ///     Writes a single ulong to the network stream
    /// </summary>
    /// <param name="value">The ulong value to write</param>
    public int WriteUlong(ulong value)
    {
        return WriteToBuffer(BitConverter.GetBytes(value));
    }
    /// <summary>
    ///     Writes a single ushort to the network stream
    /// </summary>
    /// <param name="value">The ushort value to write</param>
    public int WriteUshort(ushort value)
    {
        return WriteToBuffer(BitConverter.GetBytes(value));
    }

    /// <summary>
    ///     Helper function used to verify the BufferMode has been set to Write, as well as writing within the bounds of the
    ///     stream
    /// </summary>
    /// <param name="itemBuffer">The byte buffer for the requested variable to be written to the stream</param>
    /// <returns>The number of bytes successfully written to the stream</returns>
    private int WriteToBuffer(byte[] itemBuffer)
    {
        if (BufferMode != MessageBufferMode.Write)
        {
            LogManager
                .LogError("Attempting to write to a buffer not assigned for writing",
                          nameof(UdpWriter));

            return -1;
        }

        if (Length + itemBuffer.Length < _maximumMessageBufferSize)
        {
            if (BitConverter.IsLittleEndian && itemBuffer.Length > 1)
            {
                itemBuffer = itemBuffer.Reverse().ToArray();
            }

            itemBuffer.CopyTo(Buffer, Length);
            Length += itemBuffer.Length;
            return itemBuffer.Length;
        }

        LogManager
            .LogError($"Attempting to write {Length + itemBuffer.Length - _maximumMessageBufferSize} bytes past the end of the buffer",
                      nameof(UdpWriter));

        return -1;
    }
}