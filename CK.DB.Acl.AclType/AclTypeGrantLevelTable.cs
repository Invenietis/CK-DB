using CK.Setup;
using CK.SqlServer.Setup;

namespace CK.DB.Acl.AclType
{
    [SqlTable( "tAclTypeGrantLevel", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    public abstract class AclTypeGrantLevelTable : SqlTable
    {
        void Construct( AclTypeTable acl )
        {
        }
    }
}
