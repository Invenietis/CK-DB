using CK.Setup;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.User.UserPassword
{
    /// <summary>
    /// Package that adds a user password support. 
    /// </summary>
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    public class Package : SqlPackage
    {
        void Construct( Actor.Package actorPackage )
        {
        }

        /// <summary>
        /// Gets or sets an optional <see cref="IUserPasswordMigrator"/>
        /// that will be used to migrate from previous password management 
        /// implementations.
        /// </summary>
        public IUserPasswordMigrator PasswordMigrator { get; set; }
    }
}
