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
    [Versions("1.0.0,1.0.1")]
    [SqlObjectItem( "transform:vUserAuthProvider" )]
    public class Package : SqlPackage
    {
        void StObjConstruct( Actor.Package actorPackage, Auth.Package auth )
        {
        }

        /// <summary>
        /// Gets or sets an optional <see cref="IUserPasswordMigrator"/>
        /// that will be used to migrate from previous password management 
        /// implementations.
        /// </summary>
        public IUserPasswordMigrator PasswordMigrator { get; set; }

        /// <summary>
        /// Creates a new <see cref="IPasswordHasher"/> configured with the
        /// current <see cref="UserPasswordTable.HashIterationCount"/>.
        /// </summary>
        /// <returns>A password hasher.</returns>
        public IPasswordHasher CreatePasswordHasher() => new PasswordHasher(UserPasswordTable.HashIterationCount);

    }
}
