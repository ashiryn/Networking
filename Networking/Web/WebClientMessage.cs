namespace FluffyVoid.Networking.Web;

/// <summary>
///     Message class used to send HTTP messages to their endpoint
/// </summary>
public class WebClientMessage : WebMessage
{
    /// <summary>
    ///     Request callback that will be called when the message has received a response back
    /// </summary>
    public event Action<int, string>? Callback;

    /// <summary>
    ///     Constructor used to initialize the message
    /// </summary>
    public WebClientMessage()
    {
    }
    /// <summary>
    ///     Constructor used to initialize the message
    /// </summary>
    /// <param name="method">The HTTP delivery method</param>
    /// <param name="url">The url endpoint for the message</param>
    public WebClientMessage(WebMethod method, string url)
        : base(method, url)
    {
    }
    /// <summary>
    ///     Constructor used to initialize the message
    /// </summary>
    /// <param name="method">The HTTP delivery method</param>
    /// <param name="url">The url endpoint for the message</param>
    /// <param name="requestId">A custom id to assign to the message</param>
    public WebClientMessage(WebMethod method, string url, int requestId)
        : base(method, url, requestId)
    {
    }
    /// <summary>
    ///     Copy constructor that can build a copy of the message but updates the HTTP request properly
    /// </summary>
    /// <param name="request">The request to use in building the message</param>
    /// <param name="callback">The callback to assign to the message</param>
    /// <param name="failureCount">The current number of failed attempts for this message</param>
    protected WebClientMessage(HttpRequestMessage request,
                               Action<int, string>? callback, int failureCount)
    {
        FailureCount = failureCount;
        Request = request.Clone();
        Callback = callback;
    }

    /// <summary>
    ///     Deep copies the message and returns the copy
    /// </summary>
    /// <returns>The deeply copied web message</returns>
    public override WebMessage DeepClone()
    {
        return new WebClientMessage(Request, Callback, FailureCount);
    }
    /// <summary>
    ///     Processes the message response when the endpoint returns its reply
    /// </summary>
    /// <param name="message">The http message that was received</param>
    /// <param name="payload">The content of the message</param>
    public override void Process(HttpResponseMessage message, string payload)
    {
        Callback?.Invoke(RequestId, payload);
    }
}
/// <summary>
///     Message class used to send HTTP messages to their endpoint
/// </summary>
public class WebClientMessage<T> : WebMessage<T>
    where T : Enum
{
    /// <summary>
    ///     Request callback that will be called when the message has received a response back
    /// </summary>
    public event Action<int, string>? Callback;

    /// <summary>
    ///     Constructor used to initialize the message
    /// </summary>
    public WebClientMessage()
    {
    }
    /// <summary>
    ///     Constructor used to initialize the message
    /// </summary>
    /// <param name="method">The HTTP delivery method</param>
    /// <param name="url">The url endpoint for the message</param>
    public WebClientMessage(WebMethod method, string url)
        : base(method, url)
    {
    }
    /// <summary>
    ///     Constructor used to initialize the message
    /// </summary>
    /// <param name="method">The HTTP delivery method</param>
    /// <param name="url">The url endpoint for the message</param>
    /// <param name="requestId">A custom id to assign to the message</param>
    public WebClientMessage(WebMethod method, string url, int requestId)
        : base(method, url, requestId)
    {
    }
    /// <summary>
    ///     Copy constructor that can build a copy of the message but updates the HTTP request properly
    /// </summary>
    /// <param name="request">The request to use in building the message</param>
    /// <param name="callback">The callback to assign to the message</param>
    /// <param name="identity">The api identity/type of message when using multiple apis</param>
    /// <param name="failureCount">The current number of failed attempts for this message</param>
    protected WebClientMessage(HttpRequestMessage request,
                               Action<int, string>? callback, T? identity,
                               int failureCount)
    {
        FailureCount = failureCount;
        Request = request.Clone();
        Callback = callback;
        Identity = identity;
    }

    /// <summary>
    ///     Deep copies the message and returns the copy
    /// </summary>
    /// <returns>The deeply copied web message</returns>
    public override WebMessage DeepClone()
    {
        return new WebClientMessage<T>(Request, Callback, Identity,
                                       FailureCount);
    }
    /// <summary>
    ///     Processes the message response when the endpoint returns its reply
    /// </summary>
    /// <param name="message">The http message that was received</param>
    /// <param name="payload">The content of the message</param>
    public override void Process(HttpResponseMessage message, string payload)
    {
        Callback?.Invoke(RequestId, payload);
    }
}