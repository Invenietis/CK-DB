using CK.Core;

namespace CK.DB.Acl.AclType
{
    /// <summary>
    /// This table holds the allowed GrantLevel for each AclTypeId.
    /// </summary>
    [SqlTable( "tAclTypeGrantLevel", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    public abstract class AclTypeGrantLevelTable : SqlTable
    {
        void StObjConstruct( AclTypeTable aclType )
        {
        }

    }
}
