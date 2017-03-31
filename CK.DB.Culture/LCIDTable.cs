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
    /// <summary>
    /// This table holds the LCID values.
    /// LCID are integers between 0 and 0xFFFF.
    /// </summary>
    [SqlTable( "tLCID", Package = typeof( Package ) )]
    [Versions( "1.0.1, 1.0.2" )]
    [SqlObjectItem( "vLCID" )]
    public class LCIDTable : SqlTable
    {
        void StObjConstruct( XLCIDTable xlcid )
        {
        }

    }
}
