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
        /// Can be empty. Must not start with a '.' nor with "Oidc" that is
        /// reserved to identify the provider.
        /// </summary>
        string SchemeSuffix { get; set; }

        /// <summary>
        /// Gets or sets the Subject identifier.
        /// </summary>
        string Sub { get; set; }
    }

    public static class UserOidcInfoExtension
    {
        public static string GetScheme( this IUserOidcInfo @this )
        {
            return string.IsNullOrEmpty( @this.SchemeSuffix )
                    ? "Oidc"
                    : "Oidc." + @this.SchemeSuffix;
        }
    }

}