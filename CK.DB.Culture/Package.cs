using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Culture
{
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    public abstract partial class Package : SqlPackage
    {
    }
}
