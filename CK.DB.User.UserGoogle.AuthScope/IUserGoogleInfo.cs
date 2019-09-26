namespace CK.DB.User.UserGoogle.AuthScope
{
    /// <summary>
    /// Extends <see cref="UserGoogle.IUserGoogleInfo"/> with ScopeSet identifier.
    /// </summary>
    public interface IUserGoogleInfo : UserGoogle.IUserGoogleInfo
    {
        /// <summary>
        /// Gets the scope set identifier.
        /// Note that the ScopeSetId is intrinsic: a new ScopeSetId is acquired 
        /// and set only when a new UserGoogle is created (by copy from 
        /// the default one - the ScopeSet of the UserGoogle 0).
        /// </summary>
        int ScopeSetId { get; }
    }
}