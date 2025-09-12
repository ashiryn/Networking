namespace FluffyVoid.Networking.Udp
{
    /// <summary>
    ///     Defines a contract for network message event args
    /// </summary>
    /// <typeparam name="TNetworkMessage">The type of network message the args is responsible for</typeparam>
    public interface INetworkMessageEventArgs<TNetworkMessage>
        where TNetworkMessage : INetworkMessage
    {
        /// <summary>
        ///     The message to send with the event arg
        /// </summary>
        TNetworkMessage Message { get; }
        /// <summary>
        ///     The tag associated with the message
        /// </summary>
        ushort Tag { get; }
    }
}
