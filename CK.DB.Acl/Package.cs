using CK.Core;
using System.Diagnostics.CodeAnalysis;

namespace CK.DB.Acl
{
    /// <summary>
    /// Acl package contains <see cref="AclTable"/>, <see cref="AclConfigTable"/> and <see cref="AclConfigMemoryTable"/>.
    /// </summary>
    [SqlPackage( ResourcePath = "Res", Schema = "CK" )]
    [Versions( "1.0.0" )]
    [SqlObjectItem( "transform:sUserDestroy, transform:sGroupDestroy" )]
    public abstract class Package : SqlPackage
    {
        void StObjConstruct( Actor.Package actorPackage )
        {
        }

        /// <summary>
        /// Gets the AclTable.
        /// </summary>
        [InjectObject, AllowNull]
        public AclTable AclTable { get; protected set; }

        /// <summary>
        /// Gets the AclConfigTable.
        /// </summary>
        [InjectObject, AllowNull] 
        public AclConfigTable AclConfigTable { get; protected set; }

        /// <summary>
        /// Gets the AclConfigMemoryTable.
        /// </summary>
        [InjectObject, AllowNull]
        public AclConfigMemoryTable AclConfigMemoryTable { get; protected set; }

    }
}
