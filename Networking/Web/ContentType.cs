using System.ComponentModel;

namespace FluffyVoid.Networking.Web
{
    /// <summary>
    ///     Enumeration to determine the content type of a REST api data content
    /// </summary>
    public enum ContentType
    {
        /// <summary>
        ///     Content that consists of JSON data
        /// </summary>
        [Description("application/json")]
        Json,
        /// <summary>
        ///     Content that consists of Form data
        /// </summary>
        [Description("application/x-www-form-urlencoded")]
        Form
    }
}
