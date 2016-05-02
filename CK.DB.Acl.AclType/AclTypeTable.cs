using CK.Setup;
using CK.SqlServer.Setup;

namespace CK.DB.Acl.AclType
{
    [SqlTable( "tAclType", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    public abstract class AclTypeTable : SqlTable
    {
    }
}
