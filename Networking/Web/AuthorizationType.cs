namespace FluffyVoid.Networking.Web
{
    /// <summary>
    ///     Authorization types for use in creating an Authorization Header in HTTP requests
    /// </summary>
    public enum AuthorizationType
    {
        /// <summary>
        ///     Oauth authorization of Basic type
        /// </summary>
        Basic,
        /// <summary>
        ///     Oauth authorization of a Bearer type
        /// </summary>
        Bearer
    }
}
