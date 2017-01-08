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
        /// Gets or sets the Google account identifier.
        /// </summary>
        public string GoogleAccountId { get; set; }

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
        /// This is meaningful only when reading from the database and is ignored when writing.
        /// </summary>
        public DateTime LastRefreshTokenTime { get; set; }

        /// <summary>
        /// Gets whether this user info is valid for refresh: the access token may not be valid (it can even be null),
        /// but the <see cref="RefreshToken"/> and <see cref="GoogleAccountId"/> must be not null nor empty.
        /// </summary>
        public bool IsValidForRefresh => !string.IsNullOrWhiteSpace( GoogleAccountId ) && !string.IsNullOrWhiteSpace( RefreshToken );

    }
}
