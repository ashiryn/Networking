using System.Net;

namespace FluffyVoid.Networking.Udp
{
    /// <summary>
    ///     Data class for use in storing information about a connected client
    /// </summary>
    public class ClientInformation
    {
        /// <summary>
        ///     The id assigned to the client
        /// </summary>
        public short Id { get; }
        /// <summary>
        ///     The name of the client
        /// </summary>
        public string Name { get; }
        /// <summary>
        ///     The ip endpoint address of the client
        /// </summary>
        public IPEndPoint Endpoint { get; }
        /// <summary>
        ///     Whether the client is currently active via the heartbeat system
        /// </summary>
        public bool IsAlive { get; set; }

        /// <summary>
        ///     Constructor used to initialize the client information
        /// </summary>
        /// <param name="name">The name of the client</param>
        /// <param name="id">The id assigned to the client</param>
        /// <param name="endPoint">The ip endpoint address of the client</param>
        public ClientInformation(short id, string name, IPEndPoint endPoint)
        {
            Id = id;
            Name = name;
            Endpoint = endPoint;
            IsAlive = true;
        }
    }
}
