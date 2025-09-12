namespace FluffyVoid.Networking.Udp;

/// <summary>
///     Enumeration for globally shared upd tags that the udp networking utilizes
/// </summary>
public enum GlobalUdpMessageTag : ushort
{
    /// <summary>
    ///     Message tag used for client connect messages
    /// </summary>
    ClientConnected = 0,
    /// <summary>
    ///     Message tag for client disconnect messages
    /// </summary>
    ClientDisconnected,
    /// <summary>
    ///     Message tag for Pong messages
    /// </summary>
    Pong,
    /// <summary>
    ///     Message tag for Ping messages
    /// </summary>
    Ping,
    /// <summary>
    ///     Message tag for unknown client messages
    /// </summary>
    UnknownClient,

    /// <summary>
    ///     Reserved unused tag
    /// </summary>
    Unused = 200
}