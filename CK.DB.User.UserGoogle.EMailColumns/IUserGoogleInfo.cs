
namespace CK.DB.User.UserGoogle.EMailColumns
{
    /// <summary>
    /// Extends <see cref="UserGoogle.IUserGoogleInfo"/> with email related information.
    /// </summary>
    public interface IUserGoogleInfo : UserGoogle.IUserGoogleInfo
    {
        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        string? EMail { get; set; }

        /// <summary>
        /// Gets or sets whether the email address has been verified by Google.
        /// </summary>
        bool? EMailVerified { get; set; }
    }
}
