using CK.Setup;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Res.ResName
{
    /// <summary>
    /// This package brings the resource name support (a path-based hierarchy).
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
        /// Gets the CK.tResName table.
        /// </summary>
        [InjectContract]
        public ResNameTable ResNameTable { get; protected set; }
    }
}
