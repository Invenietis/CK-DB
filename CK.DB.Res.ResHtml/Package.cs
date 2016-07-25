using CK.Setup;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Res.ResHtml
{
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    public class Package : SqlPackage
    {
        void Construct( Res.Package resource )
        {
        }

        [InjectContract]
        public ResTable ResTable { get; protected set; }

        [InjectContract]
        public ResHtmlTable ResHtmlTable { get; protected set; }
    }
}
