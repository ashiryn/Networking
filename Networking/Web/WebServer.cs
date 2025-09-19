using System.Net;
using System.Net.WebSockets;
using FluffyVoid.Logging;
using FluffyVoid.Utilities;

namespace FluffyVoid.Networking.Web;

/// <summary>
///     Server class for listening for HTTP based messaging
/// </summary>
public class WebServer
{
    /// <summary>
    ///     Cancellation token to use when shutting down the client
    /// </summary>
    private readonly CancellationTokenSource? _cancelTokenSource;
    /// <summary>
    ///     The http listener for the web server
    /// </summary>
    private readonly HttpListener _httpListener;
    /// <summary>
    ///     Lock object used to ensure that the queue stays thread safe
    /// </summary>
    private readonly object _requestLock;
    /// <summary>
    ///     the queue of request message received
    /// </summary>
    private readonly Queue<WebServerMessage> _requestMessages;

    /// <summary>
    ///     Whether the server is currently listening for network traffic or not
    /// </summary>
    public bool IsListening { get; private set; }
    /// <summary>
    ///     Event used to notify listeners that a DELETE request has just been received
    /// </summary>

    // ReSharper disable once InconsistentNaming
    public event EventHandler<WebServerMessage>? DELETEMessageReceived;
    /// <summary>
    ///     Event used to notify listeners that a GET request has just been received
    /// </summary>

    // ReSharper disable once InconsistentNaming
    public event EventHandler<WebServerMessage>? GETMessageReceived;
    /// <summary>
    ///     Event used to notify listeners that a PATCH request has just been received
    /// </summary>

    // ReSharper disable once InconsistentNaming
    public event EventHandler<WebServerMessage>? PATCHMessageReceived;
    /// <summary>
    ///     Event used to notify listeners that a POST request has just been received
    /// </summary>

    // ReSharper disable once InconsistentNaming
    public event EventHandler<WebServerMessage>? POSTMessageReceived;
    /// <summary>
    ///     Event used to notify listeners that a PUT request has just been received
    /// </summary>

    // ReSharper disable once InconsistentNaming
    public event EventHandler<WebServerMessage>? PUTMessageReceived;
    /// <summary>
    ///     Event used to notify listeners that a new web socket client has connected to the server
    /// </summary>
    public event EventHandler<WebSocketConnectMessage>?
        WebSocketClientConnected;

    /// <summary>
    ///     Constructor used to set up the listening prefixes for the HTTP server
    /// </summary>
    /// <param name="prefixes">The list of http prefixes to bind to</param>
    public WebServer(string[] prefixes)
    {
        _requestMessages = new Queue<WebServerMessage>();
        _requestLock = new object();
        _httpListener = new HttpListener();
        _cancelTokenSource = new CancellationTokenSource();
        foreach (string prefix in prefixes)
        {
            _httpListener.Prefixes.Add(prefix);
        }
    }
    /// <summary>
    ///     Closes the server and releases all resources
    /// </summary>
    public virtual void Dispose()
    {
        _cancelTokenSource?.Dispose();
        _httpListener.Stop();
    }
    /// <summary>
    ///     Listen loop used to allow the server to listen for network traffic without blocking the main thread
    /// </summary>
    public async Task ListenAsync()
    {
        if (_cancelTokenSource == null)
        {
            return;
        }

        try
        {
            _httpListener.Start();
        }
        catch (HttpListenerException ex)
        {
            LogManager.LogException("Failed to start server", nameof(WebServer),
                                    ex: ex);
        }
        catch (Exception ex)
        {
            LogManager
                .LogException("Server threw an exception while starting the http listener",
                              nameof(WebServer), ex: ex);
        }

        IsListening = true;
        do
        {
            try
            {
                HttpListenerContext context =
                    await
                        Task.Run(() => _httpListener.GetContextAsync().WithCancellation(_cancelTokenSource.Token),
                                 _cancelTokenSource.Token);

                if (_cancelTokenSource.Token.IsCancellationRequested)
                {
                    IsListening = false;
                    return;
                }

                await ProcessMessage(context);
            }
            catch (ObjectDisposedException)
            {
                IsListening = false;
                LogManager.Log("Server shutting down->Server Disposed",
                               nameof(WebServer));
            }
            catch (TaskCanceledException)
            {
                IsListening = false;
                LogManager.Log("Server shutting down->Task Cancelled",
                               nameof(WebServer));
            }
            catch (Exception ex)
            {
                LogManager
                    .LogException("Server threw an exception while listening",
                                  nameof(WebServer), ex: ex);
            }
        } while (IsListening);
    }
    /// <summary>
    ///     Shuts down the server
    /// </summary>
    public void Shutdown()
    {
        _cancelTokenSource?.Cancel();
        Dispose();
    }
    /// <summary>
    ///     Update method that dispatches any received messages via events
    /// </summary>
    /// <param name="dt">The amount of time since the last update call</param>
    public virtual void Update(float dt)
    {
        bool requestsReady;
        lock (_requestLock)
        {
            requestsReady = _requestMessages.Any();
        }

        if (!requestsReady)
        {
            return;
        }

        WebServerMessage requestToHandle;
        lock (_requestLock)
        {
            requestToHandle = _requestMessages.Dequeue();
        }

        if (requestToHandle.IsValid)
        {
            WebMethod method =
                EnumUtility.GetValueFromDescription<WebMethod>(requestToHandle
                             .HttpMethod);

            switch (method)
            {
                case WebMethod.POST:
                    POSTMessageReceived?.Invoke(this, requestToHandle);
                    break;
                case WebMethod.GET:
                    GETMessageReceived?.Invoke(this, requestToHandle);
                    break;
                case WebMethod.PATCH:
                    PATCHMessageReceived?.Invoke(this, requestToHandle);
                    break;
                case WebMethod.DELETE:
                    DELETEMessageReceived?.Invoke(this, requestToHandle);
                    break;
                case WebMethod.PUT:
                    PUTMessageReceived?.Invoke(this, requestToHandle);
                    break;
                default:
                    LogManager
                        .LogWarning($"Received unimplemented Http Method {method}",
                                    nameof(WebServer));

                    break;
            }
        }
        else
        {
            LogManager.LogWarning("Received an invalid request from the web",
                                  nameof(WebServer));
        }
    }
    /// <summary>
    ///     Helper function used to process the incoming web message
    /// </summary>
    /// <param name="context">Reference to the current http context to retrieve a web request from</param>
    private async Task ProcessMessage(HttpListenerContext context)
    {
        try
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            if (request.IsWebSocketRequest)
            {
                HttpListenerWebSocketContext ctx =
                    await context.AcceptWebSocketAsync(null);

                WebSocket webSocket = ctx.WebSocket;
                WebSocketClientConnected?.Invoke(this,
                                                 new
                                                     WebSocketConnectMessage(webSocket,
                                                              request,
                                                              response));

                return;
            }

            lock (_requestLock)
            {
                _requestMessages.Enqueue(new WebServerMessage(context, request,
                                                  response));
            }
        }
        catch (ObjectDisposedException)
        {
            LogManager.Log("Received message when server has been disposed",
                           nameof(WebServer));
        }
        catch (HttpListenerException)
        {
            LogManager.Log("Unknown listener exception", nameof(WebServer));
        }
        catch (Exception ex)
        {
            LogManager.LogException("Unhandled exception has been caught",
                                    nameof(WebServer), ex: ex);
        }
    }
}