using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.SqlServer.Setup;
using CK.Setup;

namespace CK.DB.Actor
{
    /// <summary>
    /// Basic packages with Actors, Users and Groups.
    /// </summary>
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    [Versions( "5.0.0" )]
    public abstract class Package : SqlPackage
    {
        /// <summary>
        /// Gets the GroupTable.
        /// </summary>
        [InjectContract]
        public GroupTable GroupTable { get; protected set; }

        /// <summary>
        /// Gets the UserTable.
        /// </summary>
        [InjectContract]
        public UserTable UserTable { get; protected set; }

    }
}
