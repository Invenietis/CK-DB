namespace CK.DB.User.UserGoogle.EMail
{
    public interface IUserGoogleInfoWithMail : IUserGoogleInfo
    {
        string EMail { get; set; }
        bool? EMailVerified { get; set; }
    }
}