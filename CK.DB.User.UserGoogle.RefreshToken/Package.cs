using CK.Core;

namespace CK.DB.User.UserGoogle.RefreshToken
{
    /// <summary>
    /// Package that adds RefreshToken support to Google authentication.
    /// </summary>
    [SqlPackage( ResourcePath = "Res" )]
    [Versions("1.0.0")]
    [SqlObjectItem( "transform:CK.sUserGoogleUCL" )]
    public abstract class Package : SqlPackage
    {
        void StObjConstruct( UserGoogle.UserGoogleTable table )
        {
        }
    }
}
