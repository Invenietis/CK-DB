namespace CK.DB.Auth
{

    /// <summary>
    /// Defines standard login failure code value.
    /// </summary>
    public enum KnownLoginFailureCode
    {
        /// <summary>
        /// None is used to trigger an empty login result (<see cref="LoginResult.IsEmpty"/>).
        /// This is use for <see cref="UCLMode"/> without <see cref="UCLMode.WithCheckLogin"/>
        /// or <see cref="UCLMode.WithActualLogin"/>.
        /// </summary>
        None = 0,

        /// <summary>
        /// Generic failure code used when a string reason is set but a null or 0 failure code. 
        /// </summary>
        Unspecified = 1,

        /// <summary>
        /// Used when the user is not registered in the provider. 
        /// </summary>
        UnregisteredUser = 2,

        /// <summary>
        /// Used when the user name or identifier is not valid or the user
        /// does not exist at all, regardless of any provider.
        /// </summary>
        InvalidUserKey = 3,

        /// <summary>
        /// Used when credentials submitted are not valid.
        /// This can be used for a null password as well as a failed password match.
        /// On failed password match, this should start a lockout phasis.
        /// </summary>
        InvalidCredentials = 4,

        /// <summary>
        /// Used when a provider is disabled.
        /// </summary>
        DisabledProvider = 5,

        /// <summary>
        /// Used when a a user is globally disabled.
        /// </summary>
        GloballyDisabledUser = 6,

        /// <summary>
        /// Used when a a user is disabled for a provider.
        /// </summary>
        ProviderDisabledUser = 7

    }

    /// <summary>
    /// Provides standard default message to <see cref="KnownLoginFailureCode"/> used when
    /// a reason is not provided.
    /// </summary>
    public static class KnownLoginFailureCodeExtensions
    {
        /// <summary>
        /// Gets a standard reason string for known code.
        /// </summary>
        /// <param name="c">The login failure code.</param>
        /// <returns>A default reason string.</returns>
        public static string ToKnownString( this KnownLoginFailureCode c )
        {
            switch( c )
            {
                case KnownLoginFailureCode.UnregisteredUser: return "Unregistered user.";
                case KnownLoginFailureCode.InvalidUserKey: return "Invalid user name or identifier.";
                case KnownLoginFailureCode.InvalidCredentials: return "Invalid credentials.";
                case KnownLoginFailureCode.DisabledProvider: return "Provider is disabled.";
                case KnownLoginFailureCode.GloballyDisabledUser: return "User login is disabled.";
                case KnownLoginFailureCode.ProviderDisabledUser: return "User login is disabled for the provider.";
                default: return "Unspecified reason.";
            }
        }

        /// <summary>
        /// Gets a standard reason string for known code.
        /// Defaults to "Unspecified reason.".
        /// </summary>
        /// <param name="code">The login failure code.</param>
        /// <returns>A default reason string.</returns>
        public static string ToKnownString( int code ) => ToKnownString( (KnownLoginFailureCode)code );
    }
}
