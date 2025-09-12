namespace FluffyVoid.Networking.Web
{
    /// <summary>
    ///     Defines a contract for a queueable web request
    /// </summary>
    public interface IWebInfo
    {
        /// <summary>
        ///     Any error message that may have occurred while processing the web request
        /// </summary>
        string Error { get; }
        /// <summary>
        ///     Whether the web request has finished processing
        /// </summary>
        bool IsDone { get; }
        /// <summary>
        ///     Whether the web request should await for the callback to finish
        /// </summary>
        bool AwaitCallback { get; }

        /// <summary>
        ///     Retrieves a formatted error message
        /// </summary>
        /// <returns>The formatted error message for the web request</returns>
        string GetErrorMessage();
        /// <summary>
        ///     Whether the web request has encountered an error or not
        /// </summary>
        /// <returns>True if an error occurred, otherwise false</returns>
        bool HasError();
        /// <summary>
        ///     Processes the result of the web request
        /// </summary>
        void ProcessWebResult();
        /// <summary>
        ///     Sends the web request out to its destination
        /// </summary>
        void SendWebRequest();
    }
}
