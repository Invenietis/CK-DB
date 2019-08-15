using CK.Core;
using CK.SqlServer;

namespace CK.DB.Acl.AclType
{
    public abstract partial class AclTypeTable : SqlTable
    {
        /// <summary>
        /// Creates a new Acl type.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <returns>A new AclType identifier.</returns>
        [SqlProcedure( "sAclTypeCreate" )]
        public abstract int CreateAclType( ISqlCallContext ctx, int actorId );

        /// <summary>
        /// Destroys an Acl type identifier.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="aclTypeId">Acl type identifier to destroy.</param>
        [SqlProcedure( "sAclTypeDestroy" )]
        public abstract void DestroyAclType( ISqlCallContext ctx, int actorId, int aclTypeId );

        /// <summary>
        /// Sets or clears the ConstrainedGrantLevel bit for an AclType.
        /// When setting it, all Acl that are bound to this type MUST not be cofigured with any GrantLevel
        /// that are not allowed by the type.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="aclTypeId">Acl type identifier to configure.</param>
        /// <param name="set">True to set the ConstrainedGrantLevel bit, false to clear it.</param>
        [SqlProcedure( "sAclTypeConstrainedGrantLevelSet" )]
        public abstract void SetConstrainedGrantLevel( ISqlCallContext ctx, int actorId, int aclTypeId, bool set );

        /// <summary>
        /// Adds or removes a GrantLevel for this type.
        /// When the type is constrained, no Acl that are bound it can have GrantLevel that are not allowed.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="aclTypeId">Acl type identifier to configure.</param>
        /// <param name="grantLevel">GrantLevel to add or remove.</param>
        /// <param name="set">True to add the level, false to clear it.</param>
        [SqlProcedure( "sAclTypeGrantLevelSet" )]
        public abstract void SetGrantLevel( ISqlCallContext ctx, int actorId, int aclTypeId, byte grantLevel, bool set );

        /// <summary>
        /// Creates a typed acl.
        /// </summary>
        /// <param name="ctx">Call context to use.</param>
        /// <param name="actorId">Calling actor identifier.</param>
        /// <param name="aclTypeId">The acl type identifier.</param>
        /// <returns>A new Acl identifier that is bound to the Acl type.</returns>
        [SqlProcedure( "transform:sAclCreate" )]
        public abstract int CreateAcl( ISqlCallContext ctx, int actorId, int aclTypeId );

    }
}
