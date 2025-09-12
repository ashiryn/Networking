using FluffyVoid.Utilities;

namespace FluffyVoid.Networking.Udp;

/// <summary>
///     Serializable data class that a client can send to register receiving messages for the desired tags only
/// </summary>
public class ClientMessageTagRegistration : IUdpSerializable
{
    /// <summary>
    ///     The list of tags to register to
    /// </summary>
    private List<ushort> _tags;

    /// <summary>
    ///     Constructor used to set up the tags to register
    /// </summary>
    /// <param name="tagsToRegister">Param list of tags to register</param>
    public ClientMessageTagRegistration(params ushort[] tagsToRegister)
    {
        _tags = new List<ushort>(tagsToRegister);
    }
    /// <summary>
    ///     Constructor used to set up the tags to register to via enum
    /// </summary>
    /// <param name="tagsToRegister">Param list of enum tags to register</param>
    public ClientMessageTagRegistration(params Enum[] tagsToRegister)
    {
        _tags = new List<ushort>(tagsToRegister.Select(x => x.ToUshort()));
    }
    /// <summary>
    ///     Constructor used to set up the tags to register
    /// </summary>
    /// <param name="tagsToRegister">Param list of tags to register</param>
    public ClientMessageTagRegistration(List<ushort> tagsToRegister)
    {
        _tags = new List<ushort>(tagsToRegister);
    }
    /// <summary>
    ///     Constructor used to set up the tags to register to via enum
    /// </summary>
    /// <param name="tagsToRegister">Param list of enum tags to register</param>
    public ClientMessageTagRegistration(List<Enum> tagsToRegister)
    {
        _tags = new List<ushort>(tagsToRegister.Select(x => x.ToUshort()));
    }
    /// <summary>
    ///     Deserializes the list of tags from a udp stream
    /// </summary>
    /// <param name="e">The deserializer args to use in deserializing the data</param>
    public void Deserialize(UdpDeserializerEventArgs e)
    {
        ushort count = e.Reader.ReadUshort();
        _tags = new List<ushort>();
        for (int index = 0; index < count; ++index)
        {
            _tags.Add(e.Reader.ReadUshort());
        }
    }

    /// <summary>
    ///     Serializes the list of tags to a udp stream
    /// </summary>
    /// <param name="e">The serializer args to use in serializing the data</param>
    public int Serialize(UdpSerializerEventArgs e)
    {
        int count = e.Writer.WriteUshort((ushort)_tags.Count);
        foreach (ushort currentTag in _tags)
        {
            count += e.Writer.WriteUshort(currentTag);
        }

        return count;
    }
}