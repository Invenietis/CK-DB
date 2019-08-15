using CK.Core;

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
        [InjectObject]
        public AclTable AclTable { get; protected set; }

        /// <summary>
        /// Gets the AclConfigTable.
        /// </summary>
        [InjectObject] 
        public AclConfigTable AclConfigTable { get; protected set; }

        /// <summary>
        /// Gets the AclConfigMemoryTable.
        /// </summary>
        [InjectObject]
        public AclConfigMemoryTable AclConfigMemoryTable { get; protected set; }

    }
}
