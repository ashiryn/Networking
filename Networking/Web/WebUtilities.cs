namespace FluffyVoid.Networking.Web;

/// <summary>
///     Organizational class for utilities that relate to HTTP web tasks
/// </summary>
public static class WebUtilities
{
    /// <summary>
    ///     Extension method used to deep clone an existing HTTPRequestMessage
    /// </summary>
    /// <param name="request">Reference to the existing request</param>
    /// <returns>A deep copy of the passed in request</returns>
    public static HttpRequestMessage Clone(this HttpRequestMessage request)
    {
        HttpRequestMessage result =
            new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Content = request.Content,
                Version = request.Version
            };

        foreach (KeyValuePair<string, object?> property in request.Properties)
        {
            result.Properties.Add(property);
        }

        foreach (KeyValuePair<string, IEnumerable<string>> header in request
                     .Headers)
        {
            result.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return result;
    }
}