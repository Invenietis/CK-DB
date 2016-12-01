using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.DB.User.UserGoogle
{
    /// <summary>
    /// Holds information stored for a Google user.
    /// </summary>
    public class UserGoogleInfo
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the Google account identifier.
        /// </summary>
        public string GoogleAccountId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SimpleScopes"/> helper that holds the scopes.
        /// </summary>
        public SimpleScopes Scopes { get; set; }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets expiration time of the <see cref="AccessToken"/>.
        /// </summary>
        public DateTime? AccessTokenExpirationTime { get; set; }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the last time the refresh token has been updated.
        /// This is meaningful when reading from the database.
        /// </summary>
        public DateTime LastRefreshTokenTime { get; set; }

        /// <summary>
        /// Gets whether this user info is valid: the access token may not be valid (it can even be null),
        /// the scopes can be invalid but the <see cref="RefreshToken"/> and <see cref="GoogleAccountId"/> 
        /// are not null nor empty and the <see cref="UserId"/> is positive.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return UserId >= 0
                        && !string.IsNullOrWhiteSpace( GoogleAccountId )
                        && !string.IsNullOrWhiteSpace( RefreshToken );
            }
        }

    }
}
