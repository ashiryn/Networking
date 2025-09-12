using System.Net;
using System.Net.Sockets;
using FluffyVoid.Logging;
using FluffyVoid.Utilities;

namespace FluffyVoid.Networking.Udp;

/// <summary>
///     Client class used to communicate via UDP
/// </summary>
public class NetworkUdpClient : IDisposable
{
    /// <summary>
    ///     Cancellation token to use when shutting down the client
    /// </summary>
    private CancellationTokenSource? _cancelTokenSource;

    /// <summary>
    ///     The UDPClient to use for network communication
    /// </summary>
    private UdpClient? _client;
    /// <summary>
    ///     The name of the client
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     ID that has been assigned to the client by the server
    /// </summary>
    public short Id { get; private set; }
    /// <summary>
    ///     Whether the client is currently connected or not
    /// </summary>
    public bool IsConnected { get; private set; }
    /// <summary>
    ///     Whether the server is currently listening for network traffic or not
    /// </summary>
    public bool IsListening { get; private set; }
    /// <summary>
    ///     Event raised when the client has connected to the server
    /// </summary>
    public event EventHandler<ClientConnectedEventArgs>? Connected;
    /// <summary>
    ///     Event raised when the client has disconnected from the server
    /// </summary>
    public event EventHandler<ClientDisconnectedEventArgs>? Disconnected;
    /// <summary>
    ///     Event raised when the client has received a message from the server
    /// </summary>
    public event EventHandler<UdpMessageEventArgs>? MessageReceived;
    /// <summary>
    ///     Event raised when the client has received a message from the server that it is not currently registered
    /// </summary>
    public event EventHandler<EventArgs>? ServerUnregistered;

    /// <summary>
    ///     Constructor used to create a new UDP client
    /// </summary>
    public NetworkUdpClient(string name)
    {
        Id = -1;
        Name = name;
    }
    /// <summary>
    ///     Connects to a server by host name
    /// </summary>
    /// <param name="hostName">The host name to connect to, E.G. www.ashiryn.servequake.com</param>
    /// <param name="port">The port to connect to</param>
    public void Connect(string hostName, int port)
    {
        _client = new UdpClient();
        _cancelTokenSource = new CancellationTokenSource();
        _client.Connect(hostName, port);
        IsConnected = true;
    }
    /// <summary>
    ///     Connects to a server by ip address
    /// </summary>
    /// <param name="ipAddress">The ip string address to connect to, E.G 192.168.1.1</param>
    /// <param name="port">The port to connect to</param>
    public void ConnectViaIp(string ipAddress, int port)
    {
        _client = new UdpClient();
        _cancelTokenSource = new CancellationTokenSource();
        if (IPAddress.TryParse(ipAddress, out IPAddress? connectAddress))
        {
            IPEndPoint endpoint = new IPEndPoint(connectAddress, port);
            _client.Connect(endpoint);
            IsConnected = true;
        }
    }
    /// <summary>
    ///     Disconnects the client from the server
    /// </summary>
    public void Disconnect()
    {
        if (!IsConnected)
        {
            return;
        }

        ClientConnectionInformation info =
            new ClientConnectionInformation(Name, Id, true);

        Disconnected?.Invoke(this, new ClientDisconnectedEventArgs(true, info));
        UdpMessage connectMessage =
            new UdpMessage(GlobalUdpMessageTag.ClientDisconnected);

        UdpWriter writer = connectMessage.GetWriter();
        writer.Write(info);
        Send(connectMessage);
        _cancelTokenSource?.Cancel();
        Dispose();
    }
    /// <summary>
    ///     Disconnects the client from the server asynchronously
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (!IsConnected)
        {
            return;
        }

        ClientConnectionInformation info =
            new ClientConnectionInformation(Name, Id, true);

        UdpMessage connectMessage =
            new UdpMessage(GlobalUdpMessageTag.ClientDisconnected);

        UdpWriter writer = connectMessage.GetWriter();
        writer.Write(info);
        await SendAsync(connectMessage);
        if (_cancelTokenSource != null)
        {
            await _cancelTokenSource.CancelAsync();
        }

        Dispose();
    }

    /// <summary>
    ///     Closes the client and releases all resources
    /// </summary>
    public void Dispose()
    {
        IsConnected = false;
        _cancelTokenSource?.Dispose();
        _client?.Dispose();
    }
    /// <summary>
    ///     Listen loop used to allow the client to listen for network traffic without blocking the main thread
    /// </summary>
    public async Task ListenAsync()
    {
        if (_client == null || _cancelTokenSource == null)
        {
            LogManager
                .LogError("Unable to start listening for network traffic. Client is not properly connected.",
                          nameof(NetworkUdpClient));

            return;
        }

        do
        {
            IsListening = true;
            try
            {
                UdpReceiveResult udpResult =
                    await
                        Task.Run(() => _client.ReceiveAsync().WithCancellation(_cancelTokenSource.Token),
                                 _cancelTokenSource.Token);

                if (_cancelTokenSource.Token.IsCancellationRequested)
                {
                    IsListening = false;
                    return;
                }

                await ProcessMessage(new UdpMessage(udpResult));
            }
            catch (ObjectDisposedException)
            {
                IsListening = false;
                LogManager.Log("Client shutting down->Client Disposed",
                               nameof(NetworkUdpClient));
            }
            catch (TaskCanceledException)
            {
                IsListening = false;
                LogManager.Log("Client shutting down->Task Cancelled",
                               nameof(NetworkUdpClient));
            }
            catch (Exception ex)
            {
                IsListening = false;
                LogManager
                    .LogException("Client threw an exception while listening",
                                  nameof(NetworkUdpClient), ex: ex);
            }
        } while (IsListening);
    }
    /// <summary>
    ///     Sends a message to the server
    /// </summary>
    /// <param name="message">The message to send to the server</param>
    /// <returns>The number of bytes sent out</returns>
    public int Send(UdpMessage message)
    {
        if (!IsConnected || _client == null)
        {
            return -1;
        }

        message.GetWriter().WriteRaw(BitConverter.GetBytes(Id), 0);
        return _client.Send(message.Buffer, message.Length);
    }
    /// <summary>
    ///     Sends a message to the server
    /// </summary>
    /// <param name="message">The message to send to the server</param>
    /// <returns>The number of bytes sent out</returns>
    public async Task<int> SendAsync(UdpMessage message)
    {
        if (!IsConnected || _client == null)
        {
            return -1;
        }

        message.GetWriter().WriteRaw(BitConverter.GetBytes(Id), 0);
        return await _client.SendAsync(message.Buffer, message.Length);
    }
    /// <summary>
    ///     Sends a connection message to the server with the currently registered name of the client
    /// </summary>
    /// <returns>The number of bytes sent out</returns>
    public int SendConnectionInformation()
    {
        ClientConnectionInformation info =
            new ClientConnectionInformation(Name);

        UdpMessage connectMessage =
            new UdpMessage(GlobalUdpMessageTag.ClientConnected);

        UdpWriter writer = connectMessage.GetWriter();
        writer.Write(info);
        return Send(connectMessage);
    }
    /// <summary>
    ///     Sends a connection message to the server with the currently registered name of the client
    /// </summary>
    /// <returns>The number of bytes sent out</returns>
    public async Task<int> SendConnectionInformationAsync()
    {
        ClientConnectionInformation info =
            new ClientConnectionInformation(Name);

        UdpMessage connectMessage =
            new UdpMessage(GlobalUdpMessageTag.ClientConnected);

        UdpWriter writer = connectMessage.GetWriter();
        writer.Write(info);
        return await SendAsync(connectMessage);
    }

    /// <summary>
    ///     Helper function used to process the incoming udp message
    /// </summary>
    /// <param name="message">The incoming udp message to process</param>
    private async Task ProcessMessage(UdpMessage message)
    {
        UdpReader reader = message.GetReader();
        switch (message.Tag)
        {
            case (ushort)GlobalUdpMessageTag.ClientConnected:
            {
                ClientConnectionInformation connectionInfo =
                    reader.Read<ClientConnectionInformation>();

                Id = connectionInfo.Id;
                IsConnected = true;
                Connected?.Invoke(this,
                                  new
                                      ClientConnectedEventArgs(connectionInfo));

                LogManager
                    .LogInfo($"{connectionInfo.Name} registered: {connectionInfo.Successful}",
                             nameof(NetworkUdpClient));

                break;
            }
            case (ushort)GlobalUdpMessageTag.ClientDisconnected:
            {
                ClientConnectionInformation connectionInfo =
                    reader.Read<ClientConnectionInformation>();

                IsConnected = false;
                Disconnected?.Invoke(this,
                                     new
                                         ClientDisconnectedEventArgs(connectionInfo.Successful,
                                             connectionInfo));

                LogManager
                    .LogInfo($"{connectionInfo.Name} disconnected from server Locally:{connectionInfo.Successful}",
                             nameof(NetworkUdpClient));

                break;
            }
            case (ushort)GlobalUdpMessageTag.Ping:
            {
                LogManager
                    .LogVerbose("PING message received from server, replying with PONG message",
                                nameof(NetworkUdpClient));

                await SendAsync(new UdpMessage(GlobalUdpMessageTag.Pong));
                break;
            }
            case (ushort)GlobalUdpMessageTag.UnknownClient:
            {
                ServerUnregistered?.Invoke(this, EventArgs.Empty);
                break;
            }
            default:
            {
                MessageReceived?.Invoke(this, new UdpMessageEventArgs(message));
                break;
            }
        }
    }
}