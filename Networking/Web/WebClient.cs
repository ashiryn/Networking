using System.Net;
using FluffyVoid.Logging;
using FluffyVoid.Utilities;

namespace FluffyVoid.Networking.Web;

/// <summary>
///     Rest capable client that can communicate with an HTTP endpoint
/// </summary>
public class WebClient
{
    /// <summary>
    ///     Singleton instance to aid in keeping a single http client no matter how many the project tries to create
    /// </summary>
    private static volatile HttpClient? s_instance;
    /// <summary>
    ///     Backoff table that retries a request with exponential delays
    /// </summary>
    private readonly int[] _retryBackoffTable =
    [
        0,
        1000,
        2000,
        4000
    ];

    /// <summary>
    ///     Instance variable for the singleton that ensures the singleton is always created
    /// </summary>
    protected static HttpClient Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = new HttpClient();
            }

            return s_instance;
        }
    }
    /// <summary>
    ///     Event to notify that a request has failed due to not having proper authorization
    /// </summary>
    public event EventHandler<WebMessage>? AuthorizationFailed;

    /// <summary>
    ///     Downloads a file from the requested URL and saves it at the desired filepath location
    /// </summary>
    /// <param name="url">The URL to download the file from</param>
    /// <param name="filePath">The filepath to save the file to locally</param>
    public async Task DownloadFileAsync(string url, string filePath)
    {
        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(filePath))
        {
            LogManager
                .LogError($"Unable to download file, URL or FilePath not set\nURL={url}\nFilePath={filePath}",
                          nameof(WebClient));

            return;
        }

        try
        {
            await using Stream urlStream = await Instance.GetStreamAsync(url);
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) &&
                !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using FileStream writer =
                new FileStream(filePath, FileMode.OpenOrCreate);

            await urlStream.CopyToAsync(writer);
        }
        catch (Exception e)
        {
            LogManager.LogException($"Failed to download file from {url}",
                                    nameof(WebClient), ex: e);
        }
    }

    /// <summary>
    ///     Sends a web request to its external HTTP endpoint
    /// </summary>
    /// <param name="message">The web message data to send out</param>
    public async Task SendRequestAsync(WebMessage message)
    {
        try
        {
            using HttpResponseMessage response =
                await Instance.SendAsync(message.Request);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                case HttpStatusCode.NoContent:
                case HttpStatusCode.Accepted:
                case HttpStatusCode.Redirect:
                    try
                    {
                        message.Process(response,
                                        await response.Content
                                            .ReadAsStringAsync());

                        message.Dispose();
                    }
                    catch (Exception ex)
                    {
                        LogManager
                            .LogException($"Failed to process the response sent to {message.Request.RequestUri}",
                                          nameof(WebClient), ex: ex);
                    }

                    break;
                case HttpStatusCode.Unauthorized:
                    if (_retryBackoffTable.InBounds(message.FailureCount++))
                    {
                        string reason =
                            await response.Content.ReadAsStringAsync();

                        LogManager
                            .LogError($"Oauth token has expired: {reason}, attempting to reauthenticate.",
                                      nameof(WebClient));

                        AuthorizationFailed?.Invoke(Instance, message);
                    }
                    else
                    {
                        LogManager
                            .LogWarning($"Authorization check has failed {message.FailureCount} time(s). Manual reauthorization is required.",
                                        nameof(WebClient));
                    }

                    break;
                case HttpStatusCode.RequestTimeout:
                    if (_retryBackoffTable.InBounds(message.FailureCount))
                    {
                        await Task.Delay(_retryBackoffTable
                                             [message.FailureCount++]);

                        await SendRequestAsync(message);
                    }
                    else
                    {
                        LogManager
                            .LogWarning($"{Instance.BaseAddress} is unreachable after {message.FailureCount} attempt(s). Please try again later",
                                        nameof(WebClient));
                    }

                    break;
                default:
                    LogManager
                        .Log($"Received HTTP status code of {response.StatusCode} with Content {await response.Content.ReadAsStringAsync()}. Unsure how to handle that code",
                             nameof(WebClient));

                    break;
            }
        }
        catch (Exception ex)
        {
            LogManager
                .LogException($"Failed to send {message.Request.Method} message to {message.Request.RequestUri}",
                              nameof(WebClient), ex: ex);
        }
    }
}