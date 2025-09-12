namespace FluffyVoid.Networking.Udp;

/// <summary>
///     Data class used to store information about a clients connection
/// </summary>
public class ClientConnectionInformation : IUdpSerializable
{
    /// <summary>
    ///     The id that was assigned to the client
    /// </summary>
    public short Id { get; private set; }
    /// <summary>
    ///     The name of the client
    /// </summary>
    public string Name { get; private set; }
    /// <summary>
    ///     Whether the connection was successful or not
    /// </summary>
    public bool Successful { get; private set; }

    /// <summary>
    ///     Default constructor for the data class ensuring proper deserialization
    /// </summary>
    public ClientConnectionInformation()
    {
        Name = string.Empty;
    }
    /// <summary>
    ///     Constructor used to initialize the data class with a clients name
    /// </summary>
    /// <remarks>
    ///     Most often used by the client to request a connection to the server
    /// </remarks>
    /// <param name="name">The name the client will be connecting as</param>
    public ClientConnectionInformation(string name)
    {
        Name = name;
    }
    /// <summary>
    ///     Constructor used to initialize the data class
    /// </summary>
    /// <remarks>
    ///     Most often used by the server to inform the connecting client if the connection was successful and their new client
    ///     ID
    /// </remarks>
    /// <param name="name">The name the client will be connecting as</param>
    /// <param name="id">The id that is to be assigned to the client</param>
    /// <param name="success">Whether the connection was successful or not</param>
    public ClientConnectionInformation(string name, short id, bool success)
    {
        Name = name;
        Id = id;
        Successful = success;
    }
    /// <summary>
    ///     Deserialization method used to deserialize the data in this class from a network byte stream
    /// </summary>
    /// <param name="e">The serialization event arg to use in the deserialization process</param>
    public void Deserialize(UdpDeserializerEventArgs e)
    {
        Name = e.Reader.ReadString();
        Id = e.Reader.ReadShort();
        Successful = e.Reader.ReadBool();
    }

    /// <summary>
    ///     Serialization method used to serialize the data in this class to a network byte stream
    /// </summary>
    /// <param name="e">The serialization event arg to use in the serialization process</param>
    public int Serialize(UdpSerializerEventArgs e)
    {
        int count = e.Writer.WriteString(Name);
        count += e.Writer.WriteShort(Id);
        count += e.Writer.WriteBool(Successful);
        return count;
    }
}