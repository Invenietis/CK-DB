using System;

namespace CK.DB.User.UserGoogle
{
    /// <summary>
    /// Holds information stored for a Google user.
    /// </summary>
    public interface IUserGoogleInfo
    {
        /// <summary>
        /// Gets or sets the Google account identifier.
        /// </summary>
        string GoogleAccountId { get; set; }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets expiration time of the <see cref="AccessToken"/>.
        /// </summary>
        DateTime? AccessTokenExpirationTime { get; set; }

        /// <summary>
        /// Gets or sets the last time the refresh token has been updated.
        /// This is meaningful only when reading from the database and is ignored when writing.
        /// </summary>
        DateTime LastRefreshTokenTime { get; set; }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        string RefreshToken { get; set; }
    }

    /// <summary>
    /// Extension methods for GoogleInfo.
    /// </summary>
    public static class UserGoogleInfoExtensions
    {
        /// <summary>
        /// Gets whether this user info is valid for refresh: the access token may not be valid (it can even be null),
        /// but the <see cref="IUserGoogleInfo.RefreshToken"/> and <see cref="IUserGoogleInfo.GoogleAccountId"/> must be not null nor empty.
        /// </summary>
        public static bool IsValidForRefreshAccessToken( this IUserGoogleInfo @this )
        {
            return !string.IsNullOrWhiteSpace( @this.GoogleAccountId ) && !string.IsNullOrWhiteSpace( @this.RefreshToken );
        }

    }
}