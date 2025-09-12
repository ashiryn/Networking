using System.Net;
using System.Net.WebSockets;
using System.Text;
using FluffyVoid.Logging;

namespace FluffyVoid.Networking.WebSockets;

/// <summary>
///     Client class used to communicate via WebSocket
/// </summary>
public class WebSocketClient
{
    /// <summary>
    ///     Message buffer to use when receiving messages
    /// </summary>
    private readonly byte[] _buffer = new byte[8192];
    /// <summary>
    ///     Optional header key to add to all messages
    /// </summary>
    private readonly KeyValuePair<string, string> _header;
    /// <summary>
    ///     Reconnect falloff table to ensure we don't spam a reconnection
    /// </summary>
    private readonly int[] _reconnectFalloff =
    [
        1000,
        2000,
        4000,
        8000,
        16000,
        32000,
        64000,
        128000
    ];
    /// <summary>
    ///     Cancellation token used to cancel any active tasks for the client
    /// </summary>
    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    ///     The web socket client to use for sending and receiving messages
    /// </summary>
    private ClientWebSocket? _client;
    /// <summary>
    ///     Current reconnect attempt
    /// </summary>
    private int _reconnectCounter;
    /// <summary>
    ///     The currently connected URI for the connection
    /// </summary>
    private string? _uri;

    /// <summary>
    ///     Whether to allow the client to auto-reconnect or to allow the user to handle disconnections
    /// </summary>
    public bool AutoReconnect { protected get; set; } = false;
    /// <summary>
    ///     The KeepAlive interval for the connection
    /// </summary>
    public TimeSpan KeepAliveInterval { protected get; set; } = TimeSpan.Zero;
    /// <summary>
    ///     Optional Proxy object to use when making the connection
    /// </summary>
    public IWebProxy? Proxy { protected get; set; } = null;
    /// <summary>
    ///     Optional SubProtocol for the connection to utilize
    /// </summary>
    public string? SubProtocol { protected get; set; } = null;
    /// <summary>
    ///     Event used to notify listeners that the client has connected
    /// </summary>
    public event EventHandler? Connected;
    /// <summary>
    ///     Event used to notify listeners that the client has been disconnected
    /// </summary>
    public event EventHandler<string>? Disconnected;
    /// <summary>
    ///     Event used to notify listeners that a message has been received
    /// </summary>
    public event EventHandler<string>? MessageReceived;

    /// <summary>
    ///     Default constructor used to initialize the client
    /// </summary>
    public WebSocketClient()
    {
    }
    /// <summary>
    ///     Default constructor used to initialize the client
    /// </summary>
    /// <param name="header">The header to assign to all messages</param>
    public WebSocketClient(KeyValuePair<string, string> header)
    {
        _header = header;
    }

    /// <summary>
    ///     Connects the client to the specified URI
    /// </summary>
    /// <param name="uri">The websocket address to connect to</param>
    public async Task ConnectAsync(string uri)
    {
        _client = new ClientWebSocket();
        _uri = uri;
        _client.Options.KeepAliveInterval = KeepAliveInterval;
        if (!string.IsNullOrEmpty(_header.Key) &&
            !string.IsNullOrEmpty(_header.Value))
        {
            _client.Options.SetRequestHeader(_header.Key, _header.Value);
        }

        if (!string.IsNullOrEmpty(SubProtocol))
        {
            _client.Options.AddSubProtocol(SubProtocol);
        }

        if (Proxy != null)
        {
            _client.Options.Proxy = Proxy;
        }

        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Dispose();
        }

        _cancellationTokenSource = new CancellationTokenSource();
        await _client.ConnectAsync(new Uri(uri),
                                   _cancellationTokenSource.Token);

        if (_client.State == WebSocketState.Open)
        {
            Connected?.Invoke(this, EventArgs.Empty);
            _reconnectCounter = 0;
        }
    }
    /// <summary>
    ///     Listens for incoming messages from the connected source
    /// </summary>
    public async Task ListenAsync()
    {
        if (_client == null || _cancellationTokenSource == null || _uri == null)
        {
            LogManager
                .LogError("Websocket client has not been created, unable to listen for messages. Call ConnectAsync prior to attempting to ListenAsync.",
                          nameof(WebSocketClient));

            return;
        }

        StringBuilder message = new StringBuilder();
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                //rx messages
                WebSocketReceiveResult result;
                message.Clear();
                do
                {
                    result =
                        await
                            _client
                                .ReceiveAsync(new ArraySegment<byte>(_buffer),
                                              _cancellationTokenSource.Token);

                    message.Append(Encoding.UTF8.GetString(_buffer, 0,
                                       result.Count));
                } while (!result.EndOfMessage);

                MessageReceived?.Invoke(this, message.ToString());
                if (_client.State != WebSocketState.Closed)
                {
                    continue;
                }

                if (!AutoReconnect)
                {
                    break;
                }

                if (_reconnectCounter >= _reconnectFalloff.Length)
                {
                    Disconnected?.Invoke(this,
                                         $"Websocket connection closed, auto-reconnect failed after {_reconnectFalloff.Length} attempts.");

                    return;
                }

                Disconnected?.Invoke(this,
                                     $"Websocket connection closed, attempting auto-reconnect with server in {_reconnectFalloff[_reconnectCounter] / 1000} second(s)");

                await Task.Delay(_reconnectFalloff[_reconnectCounter++]);
                await ConnectAsync(_uri);
            }
            catch (OperationCanceledException)
            {
                Disconnected?.Invoke(this,
                                     "Websocket connection shutdown by the client.");

                return;
            }
            catch (Exception ex)
            {
                Disconnected?.Invoke(this,
                                     $"Websocket connection threw an exception, shutting down the connection./n{ex.Message}");

                return;
            }
        }

        Disconnected?.Invoke(this, "Websocket connection closed.");
    }
    /// <summary>
    ///     Sends a message via the WebSocket connection
    /// </summary>
    /// <param name="message">The message to send</param>
    public async Task SendMessageAsync(string message)
    {
        if (_client == null || _cancellationTokenSource == null)
        {
            LogManager
                .LogError($"Websocket client has not been created, unable to send {message}",
                          nameof(WebSocketClient));

            return;
        }

        if (_client.State != WebSocketState.Open)
        {
            LogManager
                .LogError($"No active Websocket connection, unable to send {message}",
                          nameof(WebSocketClient));

            return;
        }

        try
        {
            await
                _client
                    .SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)),
                               WebSocketMessageType.Text, true,
                               _cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            LogManager
                .LogException($"Sending {message} via Websocket connection failed",
                              nameof(WebSocketClient), ex: ex);
        }
    }
}