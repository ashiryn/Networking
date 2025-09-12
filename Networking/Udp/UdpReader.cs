using System.Text;
using FluffyVoid.Logging;

namespace FluffyVoid.Networking.Udp;

/// <summary>
///     Udp reader class used to read values directly from a network byte stream
/// </summary>
public class UdpReader : UdpStream
{
    /// <summary>
    ///     Default constructor for initializing the udp reader
    /// </summary>
    /// <param name="buffer">The buffer to be read by the reader</param>
    /// <param name="bufferLength">The length of the buffer</param>
    public UdpReader(byte[] buffer, int bufferLength)
    {
        Position = 0;
        Length = bufferLength;
        Buffer = buffer;
        BufferMode = MessageBufferMode.Read;
    }
    /// <summary>
    ///     Constructor used to initialize an empty reader
    /// </summary>
    /// <param name="bufferMode">The buffer mode of the udp message</param>
    public UdpReader(MessageBufferMode bufferMode = MessageBufferMode.Read)
    {
        Position = Length = 0;
        Buffer = [];
        BufferMode = bufferMode;
    }
    /// <summary>
    ///     Reads a custom serializable class from the network stream
    /// </summary>
    public T Read<T>()
        where T : IUdpSerializable
    {
        T value = Activator.CreateInstance<T>();
        value.Deserialize(new UdpDeserializerEventArgs(this));
        return value;
    }
    /// <summary>
    ///     Reads a single bool from the network stream
    /// </summary>
    /// <returns>The bool value from the network stream if successful, otherwise false</returns>
    public bool ReadBool()
    {
        if (ReadFromBuffer(out byte[] itemBuffer, sizeof(bool)) > 0)
        {
            return BitConverter.ToBoolean(itemBuffer.ToArray(), 0);
        }

        LogManager.LogError("Unable to read Bool from the buffer",
                            nameof(UdpReader));

        return false;
    }

    /// <summary>
    ///     Reads a single byte from the network stream
    /// </summary>
    /// <returns>The byte value from the network stream if successful, otherwise 0</returns>
    public byte ReadByte()
    {
        if (ReadFromBuffer(out byte[] itemBuffer, sizeof(byte)) > 0)
        {
            return itemBuffer.ToArray()[0];
        }

        LogManager.LogError("Unable to read Byte from the buffer",
                            nameof(UdpReader));

        return 0;
    }
    /// <summary>
    ///     Reads a single char from the network stream
    /// </summary>
    /// <returns>The char value from the network stream if successful, otherwise 'a'</returns>
    public char ReadChar()
    {
        if (ReadFromBuffer(out byte[] itemBuffer, sizeof(char)) > 0)
        {
            return BitConverter.ToChar(itemBuffer.ToArray(), 0);
        }

        LogManager.LogError("Unable to read Char from the buffer",
                            nameof(UdpReader));

        return 'a';
    }
    /// <summary>
    ///     Reads a single double from the network stream
    /// </summary>
    /// <returns>The double value from the network stream if successful, otherwise 0</returns>
    public double ReadDouble()
    {
        if (ReadFromBuffer(out byte[] itemBuffer, sizeof(double)) > 0)
        {
            return BitConverter.ToDouble(itemBuffer.ToArray(), 0);
        }

        LogManager.LogError("Unable to read Double from the buffer",
                            nameof(UdpReader));

        return 0;
    }
    /// <summary>
    ///     Reads a single float from the network stream
    /// </summary>
    /// <returns>The float value from the network stream if successful, otherwise 0</returns>
    public float ReadFloat()
    {
        if (ReadFromBuffer(out byte[] itemBuffer, sizeof(float)) > 0)
        {
            return BitConverter.ToSingle(itemBuffer.ToArray(), 0);
        }

        LogManager.LogError("Unable to read Float from the buffer",
                            nameof(UdpReader));

        return 0;
    }
    /// <summary>
    ///     Reads a single int from the network stream
    /// </summary>
    /// <returns>The int value from the network stream if successful, otherwise 0</returns>
    public int ReadInt()
    {
        if (ReadFromBuffer(out byte[] itemBuffer, sizeof(int)) > 0)
        {
            return BitConverter.ToInt32(itemBuffer.ToArray(), 0);
        }

        LogManager.LogError("Unable to read Int from the buffer",
                            nameof(UdpReader));

        return 0;
    }
    /// <summary>
    ///     Reads a custom serializable class from the network stream
    /// </summary>
    /// <param name="value">The class to read</param>
    public void ReadInto<T>(ref T? value)
        where T : IUdpSerializable
    {
        value ??= Read<T>();
        value.Deserialize(new UdpDeserializerEventArgs(this));
    }
    /// <summary>
    ///     Reads a single long from the network stream
    /// </summary>
    /// <returns>The long value from the network stream if successful, otherwise 0</returns>
    public long ReadLong()
    {
        if (ReadFromBuffer(out byte[] itemBuffer, sizeof(long)) > 0)
        {
            return BitConverter.ToInt64(itemBuffer.ToArray(), 0);
        }

        LogManager.LogError("Unable to read Long from the buffer",
                            nameof(UdpReader));

        return 0;
    }
    /// <summary>
    ///     Reads a single short from the network stream
    /// </summary>
    /// <returns>The short value from the network stream if successful, otherwise 0</returns>
    public short ReadShort()
    {
        if (ReadFromBuffer(out byte[] itemBuffer, sizeof(short)) > 0)
        {
            return BitConverter.ToInt16(itemBuffer.ToArray(), 0);
        }

        LogManager.LogError("Unable to read Short from the buffer",
                            nameof(UdpReader));

        return 0;
    }
    /// <summary>
    ///     Reads a string from the network stream
    /// </summary>
    /// <returns>The string value from the network stream if successful, otherwise ""</returns>
    public string ReadString()
    {
        ushort bufferLength = ReadUshort();
        if (bufferLength <= 0)
        {
            return "";
        }

        if (ReadFromBuffer(out byte[] itemBuffer, bufferLength) > 0)
        {
            return Encoding.ASCII.GetString(itemBuffer.ToArray(), 0,
                                            bufferLength);
        }

        return "";
    }
    /// <summary>
    ///     Reads a single uint from the network stream
    /// </summary>
    /// <returns>The uint value from the network stream if successful, otherwise 0</returns>
    public uint ReadUint()
    {
        if (ReadFromBuffer(out byte[] itemBuffer, sizeof(uint)) > 0)
        {
            return BitConverter.ToUInt32(itemBuffer.ToArray(), 0);
        }

        LogManager.LogError("Unable to read Uint from the buffer",
                            nameof(UdpReader));

        return 0;
    }
    /// <summary>
    ///     Reads a single ulong from the network stream
    /// </summary>
    /// <returns>The ulong value from the network stream if successful, otherwise 0</returns>
    public ulong ReadUlong()
    {
        if (ReadFromBuffer(out byte[] itemBuffer, sizeof(ulong)) > 0)
        {
            return BitConverter.ToUInt64(itemBuffer.ToArray(), 0);
        }

        LogManager.LogError("Unable to read Ulong from the buffer",
                            nameof(UdpReader));

        return 0;
    }
    /// <summary>
    ///     Reads a single ushort from the network stream
    /// </summary>
    /// <returns>The ushort value from the network stream if successful, otherwise 0</returns>
    public ushort ReadUshort()
    {
        if (ReadFromBuffer(out byte[] itemBuffer, sizeof(ushort)) > 0)
        {
            return BitConverter.ToUInt16(itemBuffer.ToArray(), 0);
        }

        LogManager.LogError("Unable to read Ushort from the buffer",
                            nameof(UdpReader));

        return 0;
    }

    /// <summary>
    ///     Helper function used to verify the BufferMode has been set to Read, as well as reading within the bounds of the
    ///     stream
    /// </summary>
    /// <param name="itemBuffer">The byte buffer for the requested variable</param>
    /// <param name="readLength">The number of bytes to read from the network buffer</param>
    /// <returns>The number of bytes successfully read from the buffer</returns>
    private int ReadFromBuffer(out byte[] itemBuffer, int readLength)
    {
        if (BufferMode != MessageBufferMode.Read)
        {
            LogManager
                .LogError("Attempting to read from a buffer not assigned for reading",
                          nameof(UdpReader));

            itemBuffer = [];
            return -1;
        }

        if (Position + readLength <= Length)
        {
            itemBuffer = BitConverter.IsLittleEndian && readLength > 1
                ? Buffer.Skip(Position).Take(readLength).Reverse().ToArray()
                : Buffer.Skip(Position).Take(readLength).ToArray();

            Position += readLength;
            return readLength;
        }

        LogManager
            .LogError($"Attempting to read {Position + readLength - Length} bytes past the end of the message buffer",
                      nameof(UdpReader));

        itemBuffer = [];
        return -1;
    }
}