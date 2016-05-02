using CK.Setup;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Acl.AclType
{
    [SqlPackage( ResourcePath = "Res", Schema = "CK" )]
    [Versions( "1.0.0" )]
    public class Package : SqlPackage
    {
        void Construct( Acl.Package acl )
        {
        }
    }
}
