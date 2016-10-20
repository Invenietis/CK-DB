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
    /// Extended LCID: all LCID are XLCID but XLCID greater than 0xFFFF are pure fallbaks culture identifiers.
    /// </summary>
    [SqlTable( "tXLCID", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    [SqlObjectItem( "vXLCID" )]
    public class XLCIDTable : SqlTable
    {
    }
}
