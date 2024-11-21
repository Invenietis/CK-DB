using CK.Core;

namespace CK.DB.Acl.AclType;

/// <summary>
/// Package for AclType brings <see cref="AclTypeTable"/> and its <see cref="AclTypeGrantLevelTable"/> 
/// that holds the allowed GrantLevel for each Acl type.
/// </summary>
[SqlPackage( ResourcePath = "Res", Schema = "CK" )]
[Versions( "1.0.0" )]
public class Package : SqlPackage
{
    void StObjConstruct( Acl.Package acl )
    {
    }
}
