using CK.Core;
using System;

namespace CK.DB.User.UserOidc
{
    /// <summary>
    /// Holds information stored for a Oidc user.
    /// </summary>
    public interface IUserOidcInfo : IPoco
    {
        /// <summary>
        /// Gets or sets the suffix client scheme.
        /// Can be empty. Must not start with a '.' nor with "Oidc".
        /// </summary>
        string SchemeSuffix { get; set; }

        /// <summary>
        /// Gets or sets the Subject identifier.
        /// </summary>
        string Sub { get; set; }
    }

    /// <summary>
    /// Extension method for IUserOidcInfo.
    /// </summary>
    public static class UserOidcInfoExtension
    {
        /// <summary>
        /// Gets either "Oidc" or "Oidc.<see cref="IUserOidcInfo.SchemeSuffix"/>".
        /// </summary>
        /// <param name="this">This IUserOidcInfo.</param>
        /// <returns>The scheme.</returns>
        public static string GetScheme( this IUserOidcInfo @this )
        {
            return string.IsNullOrEmpty( @this.SchemeSuffix )
                    ? "Oidc"
                    : "Oidc." + @this.SchemeSuffix;
        }
    }

}