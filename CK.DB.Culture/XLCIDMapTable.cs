using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;

namespace CK.DB.Culture
{

    /// <summary>
    /// This table holds the culture fallbacks: an indexed list (order matters) of the 
    /// LCID to consider from the preferred one to the worst one.
    /// </summary>
    [SqlTable( "tXLCIDMap", Package = typeof( Package ) ), Versions( "1.0.0" )]
    public class XLCIDMapTable : SqlTable
    {
        void StObjConstruct( XLCIDTable xlcid, LCIDTable lcid )
        {
        }
    }
}
