namespace FluffyVoid.Networking.Udp
{
    /// <summary>
    ///     Buffer modes for network stream
    /// </summary>
    public enum MessageBufferMode
    {
        /// <summary>
        ///     Message buffer that is capable of being read from
        /// </summary>
        Read,
        /// <summary>
        ///     Message buffer that is capable of being written to
        /// </summary>
        Write
    }
}
