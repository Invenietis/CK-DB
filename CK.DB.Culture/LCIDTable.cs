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
    [SqlTable( "tLCID", Package = typeof( Package ) )]
    [Versions( "1.0.1" )]
    [SqlObjectItem( "vLCID" )]
    public abstract class LCIDTable : SqlTable
    {
        void Construct( XLCIDTable xlcid )
        {
        }

        [SqlProcedure( "sCultureRegister" )]
        public abstract void RegisterCulture( ISqlCallContext ctx, int lcid, string name, string englishName, string nativeName, int parentLCID );

        [SqlProcedure( "sCultureDestroy" )]
        public abstract void DestroyCulture( ISqlCallContext ctx, int lcid );



    }
}
