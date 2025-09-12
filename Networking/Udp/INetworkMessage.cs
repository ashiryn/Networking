namespace FluffyVoid.Networking.Udp
{
    /// <summary>
    ///     Defines a contract for all network messages
    /// </summary>
    public interface INetworkMessage
    {
        /// <summary>
        ///     Reference to the streams byte buffer
        /// </summary>
        byte[] Buffer { get; }
        /// <summary>
        ///     The number of bytes in the stream buffer
        /// </summary>
        int Length { get; }
        /// <summary>
        ///     The tag assigned to the message
        /// </summary>
        ushort Tag { get; }
        /// <summary>
        ///     Id of the client sending the message
        /// </summary>
        short Id { get; }
    }
}
