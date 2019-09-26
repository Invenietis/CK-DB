using System.Collections.Generic;

namespace CK.DB.Auth
{
    /// <summary>
    /// Defines user authentication related information.
    /// </summary>
    public interface IUserAuthInfo
    {
        /// <summary>
        /// Gets the user identifier.
        /// </summary>
        int UserId { get; }

        /// <summary>
        /// Gets the user name.
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// Gets the schemes information for this user.
        /// </summary>
        IReadOnlyList<UserAuthSchemeInfo> Schemes { get; }
    }
}
