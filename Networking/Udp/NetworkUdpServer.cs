using System.Net;
using System.Net.Sockets;
using FluffyVoid.Logging;
using FluffyVoid.Utilities;

namespace FluffyVoid.Networking.Udp;

/// <summary>
///     Server class used to communicate via UDP
/// </summary>
public class NetworkUdpServer : IDisposable
{
    /// <summary>
    ///     Cancellation token to use when shutting down the client
    /// </summary>
    private readonly CancellationTokenSource? _cancelTokenSource;
    /// <summary>
    ///     List of clients connected to the server by assigned client id
    /// </summary>
    private readonly Dictionary<int, ClientInformation> _clientList;
    /// <summary>
    ///     Client lookup table linking a name to index within the list of clients
    /// </summary>
    private readonly Dictionary<string, List<int>> _clientLookupTable;
    /// <summary>
    ///     Heartbeat system used to track active clients and ensure stale clients can be detected
    /// </summary>
    private readonly HeartBeat _clientMonitor;
    /// <summary>
    ///     List of clients that have been detected as stale and should be removed
    /// </summary>
    private readonly List<ClientConnectionInformation> _disconnectionList;
    /// <summary>
    ///     The UdpClient to use for network communication
    /// </summary>
    private readonly UdpClient _server;
    /// <summary>
    ///     Starting id to assign to connecting clients
    /// </summary>
    private short _clientIdCount;

    /// <summary>
    ///     Whether the server is currently listening for network traffic or not
    /// </summary>
    public bool IsListening { get; private set; }
    /// <summary>
    ///     Event raised when a client has connected to the server
    /// </summary>
    public event EventHandler<ClientConnectedEventArgs>? ClientConnected;
    /// <summary>
    ///     Event raised when a client has disconnected from the server
    /// </summary>
    public event EventHandler<ClientDisconnectedEventArgs>? ClientDisconnected;
    /// <summary>
    ///     Event raised when the server receives a message from a connected client
    /// </summary>
    public event EventHandler<UdpMessageEventArgs>? MessageReceived;

    /// <summary>
    ///     Constructor for the server that binds the server to a desired port
    /// </summary>
    /// <param name="port">The port the server will bind to and listen on</param>
    public NetworkUdpServer(int port)
    {
        _server = new UdpClient();
        _cancelTokenSource = new CancellationTokenSource();
        _clientMonitor = new HeartBeat(600, 10);
        _clientMonitor.PongWindowEnded += ClientMonitorOnPongWindowEnded;
        _clientMonitor.PingWindowEnded += ClientMonitorOnPingWindowEnded;
        _disconnectionList = new List<ClientConnectionInformation>();
        _clientList = new Dictionary<int, ClientInformation>();
        _clientLookupTable = new Dictionary<string, List<int>>();
        _server.ExclusiveAddressUse = false;
        _server.Client.SetSocketOption(SocketOptionLevel.Socket,
                                       SocketOptionName.ReuseAddress, true);

        _server.Client.Bind(new IPEndPoint(IPAddress.Any, port));
    }
    /// <summary>
    ///     Constructor for the server that binds the server to a desired port and allows for a custom heartbeat interval
    /// </summary>
    /// <param name="port">The port the server will bind to and listen on</param>
    /// <param name="pongInterval">The amount of time to wait before sending PONG messages</param>
    /// <param name="pingInterval">
    ///     The amount of time to wait before disconnecting clients who haven't responded with a PING
    ///     messages
    /// </param>
    public NetworkUdpServer(int port, float pongInterval, float pingInterval)
    {
        _server = new UdpClient();
        _cancelTokenSource = new CancellationTokenSource();
        _clientMonitor = new HeartBeat(pongInterval, pingInterval);
        _clientMonitor.PongWindowEnded += ClientMonitorOnPongWindowEnded;
        _clientMonitor.PingWindowEnded += ClientMonitorOnPingWindowEnded;
        _disconnectionList = new List<ClientConnectionInformation>();
        _clientList = new Dictionary<int, ClientInformation>();
        _clientLookupTable = new Dictionary<string, List<int>>();
        _server.ExclusiveAddressUse = false;
        _server.Client.SetSocketOption(SocketOptionLevel.Socket,
                                       SocketOptionName.ReuseAddress, true);

        _server.Client.Bind(new IPEndPoint(IPAddress.Any, port));
    }

    /// <summary>
    ///     Closes the server and releases all resources
    /// </summary>
    public virtual void Dispose()
    {
        _cancelTokenSource?.Dispose();
        _server.Dispose();
    }
    /// <summary>
    ///     Listen loop used to allow the server to listen for network traffic without blocking the main thread
    /// </summary>
    public virtual async Task ListenAsync()
    {
        if (_cancelTokenSource == null)
        {
            return;
        }

        IsListening = true;
        do
        {
            try
            {
                UdpReceiveResult udpResult =
                    await
                        Task.Run(() => _server.ReceiveAsync().WithCancellation(_cancelTokenSource.Token),
                                 _cancelTokenSource.Token);

                if (_cancelTokenSource.Token.IsCancellationRequested)
                {
                    IsListening = false;
                    return;
                }

                await ProcessMessage(new UdpMessage(udpResult), udpResult);
            }
            catch (ObjectDisposedException)
            {
                IsListening = false;
                LogManager.Log("Server shutting down->Server Disposed",
                               nameof(NetworkUdpServer));
            }
            catch (TaskCanceledException)
            {
                IsListening = false;
                LogManager.Log("Server shutting down->Task Cancelled",
                               nameof(NetworkUdpServer));
            }
            catch (Exception ex)
            {
                LogManager
                    .LogException("Server threw an exception while listening",
                                  nameof(NetworkUdpServer), ex: ex);
            }
        } while (IsListening);
    }
    /// <summary>
    ///     Sends a message to all connected clients
    /// </summary>
    /// <param name="message">The message to send to the client</param>
    public async Task SendAllAsync(UdpMessage message)
    {
        List<Task> sendTasks = new List<Task>();
        foreach (ClientInformation currentClient in _clientList.Values)
        {
            sendTasks.Add(_server.SendAsync(message.Buffer, message.Length,
                                            currentClient.Endpoint));
        }

        await Task.WhenAll(sendTasks.ToArray());
    }
    /// <summary>
    ///     Sends a message to the desired client by its id
    /// </summary>
    /// <param name="clientId">The client id to send the message to</param>
    /// <param name="message">The message to send to the client</param>
    /// <returns>The number of bytes sent out</returns>
    public async Task<int> SendAsync(short clientId, UdpMessage message)
    {
        if (_clientList.TryGetValue(clientId, out ClientInformation? client))
        {
            return await _server.SendAsync(message.Buffer, message.Length,
                                           client.Endpoint);
        }

        return -1;
    }
    /// <summary>
    ///     Sends a message to the desired clients by name
    /// </summary>
    /// <param name="clientName">The name of the clients to send messages to</param>
    /// <param name="message">The message to send to the client</param>
    public async Task SendAsync(string clientName, UdpMessage message)
    {
        if (!_clientLookupTable.ContainsKey(clientName))
        {
            return;
        }

        List<Task> sendTasks = new List<Task>();
        foreach (int id in _clientLookupTable[clientName])
        {
            if (_clientList.TryGetValue(id, out ClientInformation? client))
            {
                sendTasks.Add(_server.SendAsync(message.Buffer, message.Length,
                                                client.Endpoint));
            }
        }

        await Task.WhenAll(sendTasks.ToArray());
    }
    /// <summary>
    ///     Sends a message to all connected clients except for the desired client
    /// </summary>
    /// <param name="clientId">The client id to send the message to</param>
    /// <param name="message">The message to send to the client</param>
    public async Task SendOthersAsync(short clientId, UdpMessage message)
    {
        List<Task> sendTasks = new List<Task>();
        foreach (ClientInformation currentClient in _clientList.Values)
        {
            if (currentClient.Id == clientId)
            {
                return;
            }

            sendTasks.Add(_server.SendAsync(message.Buffer, message.Length,
                                            currentClient.Endpoint));
        }

        await Task.WhenAll(sendTasks.ToArray());
    }
    /// <summary>
    ///     Sends a message to all connected clients except for the desired client
    /// </summary>
    /// <param name="clientName">The name of the clients to send messages to</param>
    /// <param name="message">The message to send to the client</param>
    public async Task SendOthersAsync(string clientName, UdpMessage message)
    {
        List<Task> sendTasks = new List<Task>();
        foreach (ClientInformation currentClient in _clientList.Values)
        {
            if (currentClient.Name == clientName)
            {
                continue;
            }

            sendTasks.Add(_server.SendAsync(message.Buffer, message.Length,
                                            currentClient.Endpoint));
        }

        await Task.WhenAll(sendTasks.ToArray());
    }
    /// <summary>
    ///     Attempts to gracefully shut the server down
    /// </summary>
    public void Shutdown()
    {
        _cancelTokenSource?.Cancel();
        Dispose();
    }
    /// <summary>
    ///     Update loop to allow certain systems to be run on a normalized update loop
    /// </summary>
    /// <param name="dt">The amount of time since the last update call</param>
    public virtual async Task Update(float dt)
    {
        await _clientMonitor.Update(dt);
    }

    /// <summary>
    ///     Sends a message to an unregistered client by its id
    /// </summary>
    /// <param name="clientEndpoint">The client endpoint to send the message to</param>
    /// <param name="message">The message to send to the client</param>
    /// <returns>The number of bytes sent out</returns>
    protected async Task<int> SendToUnknownClientAsync(
        IPEndPoint clientEndpoint, UdpMessage message)
    {
        return await _server.SendAsync(message.Buffer, message.Length,
                                       clientEndpoint);
    }
    /// <summary>
    ///     Handles properly disconnecting any stale client connections that did not respond within the Ping window
    /// </summary>
    private async Task ClientMonitorOnPingWindowEnded()
    {
        _disconnectionList.Clear();
        foreach (KeyValuePair<int, ClientInformation> client in _clientList)
        {
            if (client.Value.IsAlive)
            {
                continue;
            }

            UdpMessage toSend =
                new UdpMessage(GlobalUdpMessageTag.ClientDisconnected);

            ClientConnectionInformation info =
                new ClientConnectionInformation(client.Value.Name,
                                                client.Value.Id, false);

            UdpWriter writer = toSend.GetWriter();
            writer.Write(info);
            _disconnectionList.Add(info);
            await SendAsync(client.Value.Id, toSend);
        }

        foreach (ClientConnectionInformation client in _disconnectionList)
        {
            DisconnectClient(client);
        }
    }
    /// <summary>
    ///     Handles properly sending PING messages out to all connected clients
    /// </summary>
    private async Task ClientMonitorOnPongWindowEnded()
    {
        foreach (KeyValuePair<int, ClientInformation> client in _clientList)
        {
            UdpMessage toSend = new UdpMessage(GlobalUdpMessageTag.Ping);
            client.Value.IsAlive = false;
            LogManager
                .LogVerbose($"Sending PING message to client: {client.Value.Name}",
                            nameof(NetworkUdpServer));

            await SendAsync(client.Value.Id, toSend);
        }
    }
    /// <summary>
    ///     Helper function used to remove a client entry from the corresponding lookup tables
    /// </summary>
    /// <param name="info">The client connection information to use for removal of the client</param>
    private void DisconnectClient(ClientConnectionInformation info)
    {
        if (_clientList.ContainsKey(info.Id) &&
            _clientLookupTable.ContainsKey(_clientList[info.Id].Name))
        {
            _clientLookupTable[_clientList[info.Id].Name].Remove(info.Id);
        }

        _clientList.Remove(info.Id);
        ClientDisconnected?.Invoke(this,
                                   new
                                       ClientDisconnectedEventArgs(info.Successful,
                                           info));

        LogManager.LogInfo($"{info.Name} disconnected from server",
                           nameof(NetworkUdpServer));
    }
    /// <summary>
    ///     Helper function used to register a connecting client with the server
    /// </summary>
    /// <param name="clientId">The client id that is connecting to determine if the client is a dropped client, or a new client</param>
    /// <param name="clientEndpoint">The endpoint for the client</param>
    /// <param name="connectInfo">The connection information for the connecting client</param>
    private async Task ProcessConnectionMessage(
        short clientId, IPEndPoint clientEndpoint,
        ClientConnectionInformation connectInfo)
    {
        short currentId = _clientIdCount++;
        if (_clientList.ContainsKey(clientId))
        {
            if (_clientLookupTable.ContainsKey(_clientList[clientId].Name))
            {
                _clientLookupTable[_clientList[clientId].Name].Remove(clientId);
            }

            _clientList.Remove(clientId);
        }

        _clientList[currentId] =
            new ClientInformation(currentId, connectInfo.Name, clientEndpoint);

        if (!_clientLookupTable.ContainsKey(connectInfo.Name))
        {
            _clientLookupTable[connectInfo.Name] = new List<int>();
        }

        _clientLookupTable[connectInfo.Name].Add(currentId);
        LogManager.Log($"{connectInfo.Name} just connected at {clientEndpoint}",
                       nameof(NetworkUdpServer));

        UdpMessage connectedAck =
            new UdpMessage(GlobalUdpMessageTag.ClientConnected);

        ClientConnectionInformation clientAckInfo =
            new ClientConnectionInformation(connectInfo.Name, currentId, true);

        connectedAck.GetWriter().Write(clientAckInfo);
        await SendAsync(currentId, connectedAck);
    }

    /// <summary>
    ///     Helper function used to process the incoming udp message
    /// </summary>
    /// <param name="message">The incoming udp message to process</param>
    /// <param name="incomingMessage">The incoming udp result to aid in processing</param>
    private async Task ProcessMessage(UdpMessage message,
                                      UdpReceiveResult incomingMessage)
    {
        UdpReader reader = message.GetReader();
        switch (message.Tag)
        {
            case (ushort)GlobalUdpMessageTag.ClientConnected:
            {
                ClientConnectionInformation connectionInfo =
                    reader.Read<ClientConnectionInformation>();

                ClientConnected?.Invoke(this,
                                        new
                                            ClientConnectedEventArgs(connectionInfo));

                await ProcessConnectionMessage(message.Id,
                                               incomingMessage.RemoteEndPoint,
                                               connectionInfo);

                break;
            }
            case (ushort)GlobalUdpMessageTag.ClientDisconnected:
            {
                ClientConnectionInformation connectionInfo =
                    reader.Read<ClientConnectionInformation>();

                DisconnectClient(connectionInfo);
                break;
            }
            case (ushort)GlobalUdpMessageTag.Pong:
                if (_clientList.TryGetValue(message.Id,
                                            out ClientInformation? client))
                {
                    LogManager
                        .LogVerbose($"Received PONG from client: {message.Id}",
                                    nameof(NetworkUdpServer));

                    client.IsAlive = true;
                }
                else
                {
                    await
                        SendToUnknownClientAsync(incomingMessage.RemoteEndPoint,
                                                 new
                                                     UdpMessage(GlobalUdpMessageTag
                                                         .UnknownClient));
                }

                break;
            default:
            {
                if (!_clientList.ContainsKey(message.Id))
                {
                    await
                        SendToUnknownClientAsync(incomingMessage.RemoteEndPoint,
                                                 new
                                                     UdpMessage(GlobalUdpMessageTag
                                                         .UnknownClient));

                    LogManager
                        .LogError("Message from unknown client received, discarding message...",
                                  nameof(NetworkUdpServer));
                }
                else
                {
                    MessageReceived?.Invoke(this,
                                            new UdpMessageEventArgs(message));
                }

                break;
            }
        }
    }
}