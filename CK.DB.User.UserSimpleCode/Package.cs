using CK.Core;

namespace CK.DB.User.UserSimpleCode;

/// <summary>
/// Package that adds Simple Code authentication support for users. 
/// </summary>
[SqlPackage( Schema = "CK", ResourcePath = "Res" )]
[Versions( "1.0.0" )]
[SqlObjectItem( "transform:vUserAuthProvider" )]
public class Package : SqlPackage
{
    void StObjConstruct( Actor.Package actorPackage, Auth.Package authPackage )
    {
    }
}
