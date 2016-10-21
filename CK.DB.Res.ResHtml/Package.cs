using CK.Setup;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Res.ResHtml
{
    /// <summary>
    /// Package that brings in html value (type is nvarchar(max)) for resources. 
    /// </summary>
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    public class Package : SqlPackage
    {
        void Construct( Res.Package resource )
        {
        }

        /// <summary>
        /// Gets the resource table.
        /// </summary>
        [InjectContract]
        public ResTable ResTable { get; protected set; }

        /// <summary>
        /// Gets the html text holder table.
        /// </summary>
        [InjectContract]
        public ResHtmlTable ResHtmlTable { get; protected set; }
    }
}
