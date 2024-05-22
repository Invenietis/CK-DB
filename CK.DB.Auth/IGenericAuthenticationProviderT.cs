namespace CK.DB.Auth
{

    /// <summary>
    /// Specializes <see cref="IGenericAuthenticationProvider"/> to expose its typed
    /// payload.
    /// </summary>
    /// <typeparam name="T">Type of the payload.</typeparam>
    public interface IGenericAuthenticationProvider<T> : IGenericAuthenticationProvider where T : class
    {
        /// <summary>
        /// Creates an empty payload object.
        /// </summary>
        /// <returns>A payload object.</returns>
        new T CreatePayload();
    }
}
