using CK.Setup;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Res.ResString
{
    /// <summary>
    /// Package that brings in string (of type nvarchar(400)) for resources. 
    /// </summary>
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    public class Package : SqlPackage
    {
        void StObjConstruct( Res.Package resource )
        {
        }

        /// <summary>
        /// Gets the resource table.
        /// </summary>
        [InjectContract]
        public ResTable ResTable { get; protected set; }

        /// <summary>
        /// Gets the string holder table.
        /// </summary>
        [InjectContract]
        public ResStringTable ResStringTable { get; protected set; }
    }
}
