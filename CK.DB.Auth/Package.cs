using CK.Setup;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Auth
{
    /// <summary>
    /// This package defines common abstractions and data to authentication providers.
    /// </summary>
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    [Versions( "1.0.0" )]
    [SqlObjectItem( "vUserAuthProvider" )]
    public class Package : SqlPackage
    {
        void Construct( Actor.Package actor )
        {
        }
    }
}
