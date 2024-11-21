using CK.Core;

namespace CK.DB.User.UserGoogle.EMailColumns;

/// <summary>
/// Package that adds EMails columns support to Google authentication.
/// </summary>
[SqlPackage( ResourcePath = "Res" )]
[Versions( "1.0.0" )]
[SqlObjectItem( "transform:CK.sUserGoogleUCL" )]
public abstract class Package : SqlPackage
{
    void StObjConstruct( UserGoogle.UserGoogleTable table )
    {
    }
}
