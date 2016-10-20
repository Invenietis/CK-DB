using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;
using System.Threading.Tasks;

namespace CK.DB.Acl.AclType
{
    /// <summary>
    /// This table defines the AclType. Each Acl type defines allowed GrantLevels.
    /// When ConstrainedGrantLevel bit is 1, any Acl that is bound to the AclType can only 
    /// accepts GrantLevel that are defined by its type.
    /// </summary>
    [SqlTable( "tAclType", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    [SqlObjectItem( "transform:sAclGrantSet" )]
    public abstract partial class AclTypeTable : SqlTable
    {
        void Construct( AclTable acl )
        {
        }

        /// <summary>
        /// Creates a new Acl type.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <returns>A new AclType identifier.</returns>
        [SqlProcedure( "sAclTypeCreate" )]
        public abstract Task<int> CreateAclTypeAsync( ISqlCallContext ctx, int actorId );

        /// <summary>
        /// Destroys an Acl type identifier.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="aclTypeId">Acl type identifier to destroy.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sAclTypeDestroy" )]
        public abstract Task DestroyAclTypeAsync( ISqlCallContext ctx, int actorId, int aclTypeId );

        /// <summary>
        /// Sets or clears the ConstrainedGrantLevel bit for an AclType.
        /// When setting it, all Acl that are bound to this type MUST not be cofigured with any GrantLevel
        /// that are not allowed by the type.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="aclTypeId">Acl type identifier to configure.</param>
        /// <param name="set">True to set the ConstrainedGrantLevel bit, false to clear it.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sAclTypeConstrainedGrantLevelSet" )]
        public abstract Task SetConstrainedGrantLevelAsync( ISqlCallContext ctx, int actorId, int aclTypeId, bool set );

        /// <summary>
        /// Adds or removes a GrantLevel for this type.
        /// When the type is constrained, no Acl that are bound it can have GrantLevel that are not allowed.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="aclTypeId">Acl type identifier to configure.</param>
        /// <param name="grantLevel">GrantLevel to add or remove.</param>
        /// <param name="set">True to add the level, false to clear it.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sAclTypeGrantLevelSet" )]
        public abstract Task SetGrantLevelAsync( ISqlCallContext ctx, int actorId, int aclTypeId, byte grantLevel, bool set );

        /// <summary>
        /// Creates a typed acl.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="aclTypeId">The acl type identifier.</param>
        /// <returns>A new Acl identifier that is bound to the Acl type.</returns>
        [SqlProcedure( "transform:sAclCreate" )]
        public abstract Task<int> CreateAclAsync( ISqlCallContext ctx, int actorId, int aclTypeId );

    }
}
