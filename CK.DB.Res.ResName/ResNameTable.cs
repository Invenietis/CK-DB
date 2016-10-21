using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Res.ResName
{
    /// <summary>
    /// This table holds the resource name.
    /// Resource names implement a path-based hierarchy.
    /// </summary>
    [SqlTable( "tResName", Package = typeof( Package ) )]
    [Versions( "1.0.0, 1.0.1" )]
    [SqlObjectItem( "transform:vRes, transform:sResDestroy" )]
    [SqlObjectItem( "fResNamePrefixes" )]
    [SqlObjectItem( "vResNameAllChildren, vResNameDirectChildren, vResNameParentPrefixes" )]
    public abstract partial class ResNameTable : SqlTable
    {
        /// <summary>
        /// Renames a resource, by default renaming also its children.
        /// Nothing is done if the resource does not exist or has no associated ResName.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="resId">The resource identifier to rename.</param>
        /// <param name="newName">The new resource name.</param>
        /// <param name="withChildren">
        /// False to rename only this resource and not its children: children
        /// names are left as-is as "orphans".
        /// </param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure("sResNameRename")]
        public abstract Task RenameAsync( ISqlCallContext ctx, int resId, string newName, bool withChildren = true );

        /// <summary>
        /// Creates a new resource name for an existing resource identifier.
        /// There must not be an existing resource name.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="resId">The resource identifier that must exist.</param>
        /// <param name="resName">The resource name to create.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sResNameCreate" )]
        public abstract Task CreateResNameAsync( ISqlCallContext ctx, int resId, string resName );

        /// <summary>
        /// Creates a new resource with a name.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="resName">The resource name to create.</param>
        /// <returns>The new resource identifier.</returns>
        [SqlProcedure( "sResCreateWithResName" )]
        public abstract Task<int> CreateWithResNameAsync( ISqlCallContext ctx, string resName );

        /// <summary>
        /// Destroys the resource name associated to a resource if any.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="resId">The resoource identifier.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sResNameDestroy" )]
        public abstract Task DestroyResNameAsync( ISqlCallContext ctx, int resId );

        /// <summary>
        /// Destroys all ressources which ResName start with <paramref name="resNamePrefix"/> + '.'.
        /// Since this method works on resource name, <paramref name="resNameOnly"/> defaults to true
        /// but this can be applied to the whole resources.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="resNamePrefix">Prefix of the resources to destroy.</param>
        /// <param name="resNameOnly">False to call sResDestroy (destroying the whole resources) instead of sResNameDestroy.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sResDestroyByResNamePrefix" )]
        public abstract Task DestroyByResNamePrefixAsync( ISqlCallContext ctx, string resNamePrefix, bool resNameOnly = true );

        /// <summary>
        /// Destroys all children resources (or, optionally, only their ResName part).
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="resId">The parent resource identifier.</param>
        /// <param name="resNameOnly">True to only call sResNameDestroy instead of sResDestroy.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sResDestroyResNameChildren" )]
        public abstract Task DestroyResNameChildrenAsync( ISqlCallContext ctx, int resId, bool resNameOnly = false );

        /// <summary>
        /// Destroys a resource and all its children (or, optionally, only their ResName part).
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="resId">The resource identifier to destroy, including its children.</param>
        /// <param name="resNameOnly">True to only call sResNameDestroy instead of sResDestroy.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sResDestroyWithResNameChildren" )]
        public abstract Task DestroyWithResNameChildrenAsync( ISqlCallContext ctx, int resId, bool resNameOnly = false );


    }
}
