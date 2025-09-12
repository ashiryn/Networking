using System.Net;
using System.Net.WebSockets;

namespace FluffyVoid.Networking.Web;

/// <summary>
///     Message class used to hold HTTP items useful for handling HTTP message traffic via a WebSocket connection
/// </summary>
public readonly struct WebSocketConnectMessage
{
    /// <summary>
    ///     Reference to the WebSocket connection
    /// </summary>
    public WebSocket Socket { get; }
    /// <summary>
    ///     Reference to the HTTP request
    /// </summary>
    public HttpListenerRequest Request { get; }
    /// <summary>
    ///     Reference to the HTTP Response
    /// </summary>
    public HttpListenerResponse Response { get; }
    /// <summary>
    ///     Whether the message is valid or not
    /// </summary>
    public bool IsValid =>
        Socket != null && Request != null && Response != null;

    /// <summary>
    ///     Constructor used to build the web socket connect message
    /// </summary>
    /// <param name="socket">Reference to the WebSocket connection</param>
    /// <param name="request">Reference to the HTTP request</param>
    /// <param name="response">Reference to the HTTP Response</param>
    public WebSocketConnectMessage(WebSocket socket,
                                   HttpListenerRequest request,
                                   HttpListenerResponse response)
    {
        Socket = socket;
        Request = request;
        Response = response;
    }
}