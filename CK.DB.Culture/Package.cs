using CK.Setup;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Culture
{
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    public class Package : SqlPackage
    {
        [InjectContract]
        public LCIDTable LCIDTable { get; protected set; }

        [InjectContract]
        public XLCIDTable XLCIDTable { get; protected set; }

    }
}
