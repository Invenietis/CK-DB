
namespace CK.DB.User.UserGoogle.EMailColumns
{
    /// <summary>
    /// Extends <see cref="IUserGoogleInfo"/> with email related information.
    /// </summary>
    public interface IUserGoogleInfoWithMail : IUserGoogleInfo
    {
        string EMail { get; set; }

        bool? EMailVerified { get; set; }
    }
}