using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.SqlServer.Setup;
using CK.Setup;
using CK.SqlServer;

namespace CK.DB.Zone
{
    /// <summary>
    /// This package subordinates Groups to Zones.
    /// </summary>
    [SqlPackage( ResourcePath = "Res", ResourceType = typeof(Package) )]
    [Versions( "5.0.0" )]
    public abstract class Package : Actor.Package
    {
        /// <summary>
        /// Gets the GroupTable that this package extends.
        /// </summary>
        public new GroupTable GroupTable => (GroupTable)base.GroupTable;

        /// <summary>
        /// Gets the GroupTable that this package defines.
        /// </summary>
        [InjectContract]
        public ZoneTable ZoneTable { get; protected set; }

    }
}
