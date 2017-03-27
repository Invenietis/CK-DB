using CK.Setup;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Actor.ActorEMail
{
    /// <summary>
    /// Brings <see cref="ActorEMailTable"/> to support emails' user.
    /// </summary>
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    [Versions( "1.0.0" )]
    public class Package : SqlPackage
    {
        void Construct( Actor.Package actorPackage )
        {
        }

        /// <summary>
        /// Gets the <see cref="ActorEMailTable"/>.
        /// </summary>
        public ActorEMailTable ActorEMailTable { get; protected set; }

    }
}
