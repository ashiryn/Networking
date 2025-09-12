// ReSharper disable InconsistentNaming

namespace FluffyVoid.Networking.Web
{
    /// <summary>
    ///     Enum for the different methods of delivering HTTP messages to an endpoing
    /// </summary>
    public enum WebMethod
    {
        /// <summary>
        ///     Web API POST method type that posts data to an endpoint
        /// </summary>
        POST,
        /// <summary>
        ///     Web API Get method type that retrieves data from an endpoint
        /// </summary>
        GET,
        /// <summary>
        ///     Web API Patch method type that updates data at an endpoint
        /// </summary>
        PATCH,
        /// <summary>
        ///     Web API Delete method type that removes data from an endpoint
        /// </summary>
        DELETE,
        /// <summary>
        ///     Web API Put method type that saves data to an endpoint
        /// </summary>
        PUT
    }
}
