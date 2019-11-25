
namespace CK.DB.User.UserGoogle.Profile
{
    /// <summary>
    /// Extends <see cref="UserGoogle.IUserGoogleInfo"/> with profile related information.
    /// </summary>
    public interface IUserGoogleInfo : UserGoogle.IUserGoogleInfo
    {
        /// <summary>
        /// Gets or sets the first name, that is the OIDC "Given Name" (http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname).
        /// </summary>
        string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name, that is the OIDC "Surname" (http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname).
        /// </summary>
        string LastName { get; set; }

        /// <summary>
        /// Gets or sets the name provided by Google (the claim type is only "name" and just like <see cref="Picture"/> it is not a standard OIDC property).
        /// </summary>
        string UserName { get; set; }

        /// <summary>
        /// Gets or sets the "picture" claim that is not a standard OIDC property.
        /// </summary>
        string PictureUrl { get; set; }

    }
}
