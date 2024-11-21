using CK.Core;

namespace CK.DB.User.NamedUser;

/// <summary>
/// Package that adds a Firstname and a Lastname.
/// </summary>
[SqlPackage( Schema = "CK", ResourcePath = "Res" )]
[Versions( "1.0.0" )]
public class Package : SqlPackage
{
    void StObjConstruct( Actor.Package actorPackage ) { }
}
