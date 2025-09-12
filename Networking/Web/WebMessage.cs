using System.Net.Http.Headers;
using System.Text;
using FluffyVoid.Logging;
using FluffyVoid.Utilities;
using FluffyVoid.Utilities.Cloneable;

namespace FluffyVoid.Networking.Web;

/// <summary>
///     Base class for all HTTP web messages
/// </summary>
public class WebMessage : IDisposable, IDeepCloneable<WebMessage>
{
    /// <summary>
    ///     The number of failure attempts for the message
    /// </summary>
    public int FailureCount { get; set; }
    /// <summary>
    ///     The cached HTTP request message
    /// </summary>
    public HttpRequestMessage Request { get; protected set; }
    /// <summary>
    ///     The id of the request
    /// </summary>
    public int RequestId { get; set; }

    /// <summary>
    ///     Constructor that does no initialization, for use by inherited classes when wanting to do full initialization at
    ///     that level
    /// </summary>
    public WebMessage()
    {
        Request = null!;
    }
    /// <summary>
    ///     Constructor used to initialize the web message
    /// </summary>
    /// <param name="method">The HTTP delivery method</param>
    /// <param name="url">The url endpoint for the message</param>
    /// <param name="requestId">A custom id to assign to the message</param>
    public WebMessage(WebMethod method, string url, int requestId = -1)
    {
        FailureCount = 0;
        RequestId = requestId;
        Request =
            new HttpRequestMessage(new HttpMethod(method.ToString()), url);
    }
    /// <summary>
    ///     Constructor used to initialize the web message
    /// </summary>
    /// <param name="request">The HTTP request to store in the web message</param>
    /// <param name="failureCount">The number of failure attempts for the message</param>
    /// <param name="requestId">A custom id to assign to the message</param>
    protected WebMessage(HttpRequestMessage request, int failureCount,
                         int requestId = -1)
    {
        FailureCount = failureCount;
        RequestId = requestId;
        Request = request.Clone();
    }

    /// <summary>
    ///     Adds an authorization header to the web message
    /// </summary>
    /// <param name="type">The authorization type to use</param>
    /// <param name="value">The value for the authorization header</param>
    public virtual void AddAuthorizationHeader(AuthorizationType type,
                                               string value)
    {
        switch (type)
        {
            case AuthorizationType.Basic:
            {
                string base64EncodedAuthenticationString =
                    Convert.ToBase64String(Encoding.ASCII.GetBytes(value));

                Request.Headers.Authorization =
                    new AuthenticationHeaderValue(type.GetDescription(),
                                                  base64EncodedAuthenticationString);

                break;
            }
            case AuthorizationType.Bearer:
            {
                Request.Headers.Authorization =
                    new AuthenticationHeaderValue(type.GetDescription(), value);

                break;
            }
            default:
            {
                LogManager
                    .LogWarning($"Unhandled authorization type [{type}], manual header assignment required until implemented",
                                nameof(WebMessage));

                break;
            }
        }
    }

    /// <summary>
    ///     Adds the passed in content to the message with the desired content type
    /// </summary>
    /// <param name="content">The content to set as the payload</param>
    /// <param name="type">The content type to set the payload as</param>
    public virtual void AddContent(string content, ContentType type)
    {
        AddContent(content, type, Encoding.UTF8);
    }
    /// <summary>
    ///     Adds the passed in content to the message with the desired content type and encoding style
    /// </summary>
    /// <param name="content">The content to set as the payload</param>
    /// <param name="type">The content type to set the payload as</param>
    /// <param name="contentEncoding">The encoding to use when setting the content</param>
    public virtual void AddContent(string content, ContentType type,
                                   Encoding contentEncoding)
    {
        Request.Content =
            new StringContent(content, contentEncoding, type.GetDescription());
    }
    /// <summary>
    ///     Adds a header KVP to the message
    /// </summary>
    /// <param name="key">The key of the header</param>
    /// <param name="value">The value of the header</param>
    public virtual void AddHeader(string key, string value)
    {
        Request.Headers.TryAddWithoutValidation(key, value);
    }
    /// <summary>
    ///     Adds a User-Agent header to the web message
    /// </summary>
    /// <param name="name">The name of the product</param>
    /// <param name="version">The version of the product</param>
    /// <param name="comment">The description, enclosed by (), of the product</param>
    public virtual void AddUserAgent(string name, string version,
                                     string comment)
    {
        Request.Headers.UserAgent
               .Add(new ProductInfoHeaderValue(name, version));

        Request.Headers.UserAgent.Add(new ProductInfoHeaderValue(comment));
    }
    /// <summary>
    ///     Deeply clones the message and returns a copy
    /// </summary>
    /// <returns>The deeply copied version of this message</returns>
    public virtual WebMessage DeepClone()
    {
        return new WebMessage(Request, FailureCount, RequestId);
    }
    /// <summary>
    ///     Disposes of any resources the message has created
    /// </summary>
    public virtual void Dispose()
    {
        Request.Dispose();
    }

    /// <summary>
    ///     Initializes the WebMessage with values needed to send a request
    /// </summary>
    /// <param name="method">The HTTP delivery method</param>
    /// <param name="url">The url endpoint for the message</param>
    /// <param name="requestId">A custom id to assign to the message</param>
    public virtual void Initialize(WebMethod method, string url,
                                   int requestId = -1)
    {
        FailureCount = 0;
        RequestId = requestId;
        Request =
            new HttpRequestMessage(new HttpMethod(method.ToString()), url);
    }
    /// <summary>
    ///     Processes the message response when the endpoint returns its reply
    /// </summary>
    /// <param name="message">The http message that was received</param>
    /// <param name="payload">The content of the message</param>
    public virtual void Process(HttpResponseMessage message, string payload)
    {
        LogManager
            .LogInfo($"Received response {message.StatusCode}:{message.ReasonPhrase} from [{message.RequestMessage?.RequestUri}]: {payload}",
                     nameof(WebMessage));
    }
    /// <summary>
    ///     Removes a header KVP from the message by key
    /// </summary>
    /// <param name="key">The key of the header to remove</param>
    /// <returns>True if the header was successfully removed, otherwise false</returns>
    public bool RemoveHeader(string key)
    {
        return Request.Headers.Remove(key);
    }
    /// <summary>
    ///     Updates an existing header KVP with the new KVP passed in
    /// </summary>
    /// <param name="key">The key of the header to replace</param>
    /// <param name="value">The new value of the header</param>
    public void UpdateHeader(string key, string value)
    {
        RemoveHeader(key);
        AddHeader(key, value);
    }
    /// <summary>
    ///     Deeply clones the message and returns a copy
    /// </summary>
    /// <returns>The deeply copied version of this message</returns>
    object IDeepCloneable.DeepClone()
    {
        return DeepClone();
    }
}
/// <summary>
///     Base class for all HTTP web messages with different oauth points
/// </summary>
public class WebMessage<T> : WebMessage
    where T : Enum
{
    /// <summary>
    ///     The api identity/type of message when using multiple apis
    /// </summary>
    public T? Identity { get; set; }

    /// <summary>
    ///     Constructor that does no initialization, for use by inherited classes when wanting to do full initialization at
    ///     that level
    /// </summary>
    public WebMessage()
    {
        Identity = default;
    }
    /// <summary>
    ///     Constructor used to initialize the web message
    /// </summary>
    /// <param name="method">The HTTP delivery method</param>
    /// <param name="url">The url endpoint for the message</param>
    /// <param name="requestId">A custom id to assign to the message</param>
    public WebMessage(WebMethod method, string url, int requestId = -1)
        : base(method, url, requestId)
    {
        Identity = default;
    }
    /// <summary>
    ///     Constructor used to initialize the web message
    /// </summary>
    /// <param name="method">The HTTP delivery method</param>
    /// <param name="url">The url endpoint for the message</param>
    /// <param name="identity">The api identity/type of message when using multiple apis</param>
    /// <param name="requestId">A custom id to assign to the message</param>
    public WebMessage(WebMethod method, string url, T? identity,
                      int requestId = -1)
        : base(method, url, requestId)
    {
        Identity = identity;
    }
    /// <summary>
    ///     Constructor used to initialize the web message
    /// </summary>
    /// <param name="request">The HTTP request to store in the web message</param>
    /// <param name="identity">The api identity/type of message when using multiple apis</param>
    /// <param name="failureCount">The number of failure attempts for the message</param>
    /// <param name="requestId">A custom id to assign to the message</param>
    protected WebMessage(HttpRequestMessage request, T? identity,
                         int failureCount, int requestId = -1)
        : base(request, failureCount, requestId)
    {
        Identity = identity;
    }

    /// <summary>
    ///     Deeply clones the message and returns a copy
    /// </summary>
    /// <returns>The deeply copied version of this message</returns>
    public override WebMessage DeepClone()
    {
        return new WebMessage<T>(Request, Identity, FailureCount, RequestId);
    }
    /// <summary>
    ///     Initializes the WebMessage with values needed to send a request
    /// </summary>
    /// <param name="method">The HTTP delivery method</param>
    /// <param name="url">The url endpoint for the message</param>
    /// <param name="identity">The api identity/type of message when using multiple apis</param>
    /// <param name="requestId">A custom id to assign to the message</param>
    public void Initialize(WebMethod method, string url, T identity,
                           int requestId = -1)
    {
        base.Initialize(method, url, requestId);
        Identity = identity;
    }
}