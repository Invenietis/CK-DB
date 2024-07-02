using CK.Core;

namespace CK.DB.User.UserGoogle
{
    /// <summary>
    /// Holds information stored for a Google user.
    /// </summary>
    public interface IUserGoogleInfo : IPoco
    {
        /// <summary>
        /// Gets or sets the Google account identifier.
        /// </summary>
        string? GoogleAccountId { get; set; }
    }

}
