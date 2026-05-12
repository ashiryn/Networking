using System.Net;

namespace FluffyVoid.Networking.Web
{
    /// <summary>
    ///     Message class used to hold HTTP items useful for handling HTTP message traffic
    /// </summary>
    public readonly struct WebServerMessage
    {
        /// <summary>
        ///     Reference to the HTTP Context
        /// </summary>
        public HttpListenerContext Context { get; }
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
        public bool IsValid => Context != null && Request != null && Response != null;
        /// <summary>
        ///     The currently assigned HTTP method the message is using
        /// </summary>
        public string HttpMethod => Request.HttpMethod;

        /// <summary>
        ///     Constructor used to build the server message
        /// </summary>
        /// <param name="context">Reference to the HTTP Context</param>
        /// <param name="request">Reference to the HTTP request</param>
        /// <param name="response">Reference to the HTTP Response</param>
        public WebServerMessage(HttpListenerContext context, HttpListenerRequest request, HttpListenerResponse response)
        {
            Context = context;
            Request = request;
            Response = response;
        }
    }
}