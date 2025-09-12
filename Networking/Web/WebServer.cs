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
    ///     The http listener for the web server
    /// </summary>
    private readonly HttpListener _httpListener;
    /// <summary>
    ///     The listener thread for the server
    /// </summary>
    private readonly Thread? _listenerThread;
    /// <summary>
    ///     Lock object used to ensure that the queue stays thread safe
    /// </summary>
    private readonly Lock _requestLock;
    /// <summary>
    ///     the queue of request message received
    /// </summary>
    private readonly Queue<WebServerMessage> _requestMessages;
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
    ///     Constructor used to setup the listening prefixes for the HTTP server
    /// </summary>
    /// <param name="prefixes">The list of http prefixes to bind to</param>
    public WebServer(string[] prefixes)
    {
        _requestMessages = new Queue<WebServerMessage>();
        _requestLock = new Lock();
        _httpListener = new HttpListener();
        foreach (string prefix in prefixes)
        {
            _httpListener.Prefixes.Add(prefix);
        }

        try
        {
            _httpListener.Start();
            _listenerThread = new Thread(StartListening);
            _listenerThread.Start();
        }
        catch (HttpListenerException ex)
        {
            LogManager.LogException("Failed to start server", nameof(WebServer),
                                    ex: ex);
        }
    }
    /// <summary>
    ///     Shuts down the server
    /// </summary>
    public void Shutdown()
    {
        _httpListener.Stop();
        _listenerThread?.Join(2000);
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
    ///     Async result callback that parses and builds the WebServerMessage to use be queued
    /// </summary>
    /// <param name="result">The async result object passed in with the callback</param>
    private void MessageReceived(IAsyncResult result)
    {
        if (result.AsyncState is not HttpListener listener)
        {
            return;
        }

        try
        {
            HttpListenerContext context = listener.EndGetContext(result);
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            if (request.IsWebSocketRequest)
            {
                Task<HttpListenerWebSocketContext> ctx =
                    context.AcceptWebSocketAsync(null);

                WebSocket webSocket = ctx.Result.WebSocket;
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

    /// <summary>
    ///     Starts the listener thread for the server to start listening for HTTP messages
    /// </summary>
    private void StartListening()
    {
        try
        {
            while (_httpListener.IsListening)
            {
                IAsyncResult result =
                    _httpListener.BeginGetContext(MessageReceived,
                                                  _httpListener);

                result.AsyncWaitHandle.WaitOne();
            }
        }
        catch (ObjectDisposedException)
        {
            LogManager.Log("Server shutting down->Server Disposed",
                           nameof(WebServer));
        }
        catch (Exception ex)
        {
            LogManager
                .LogException("Server threw an exception while listening",
                              nameof(WebServer), ex: ex);
        }
    }
}