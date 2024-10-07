using CK.Core;
using CK.SqlServer;
using System.Threading.Tasks;

namespace CK.DB.Res.ResName;

/// <summary>
/// This table holds the resource name.
/// Resource names implement a path-based hierarchy.
/// </summary>
[SqlTable( "tResName", Package = typeof( Package ) )]
[Versions( "1.0.0, 1.0.1, 1.0.2" )]
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
    [SqlProcedure( "sResNameRename" )]
    public abstract Task RenameAsync( ISqlCallContext ctx, int resId, string newName, bool withChildren = true );

    /// <summary>
    /// Renames a resource, by default renaming also its children.
    /// Nothing is done if the resource does not exist or has no associated ResName.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="oldName">The resource name to rename.</param>
    /// <param name="newName">The new resource name.</param>
    /// <param name="withChildren">
    /// False to rename only this resource and not its children: children
    /// names are left as-is as "orphans".
    /// </param>
    /// <returns>The awaitable.</returns>
    [SqlProcedure( "sResNameRenameResName" )]
    public abstract Task RenameAsync( ISqlCallContext ctx, string oldName, string newName, bool withChildren = true );

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
    /// Destroys the resource name associated to a resource identifier if any.
    /// This doesn't destroy the resource itself, only the name.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="resId">The resoource identifier.</param>
    /// <returns>The awaitable.</returns>
    [SqlProcedure( "sResNameDestroy" )]
    public abstract Task DestroyResNameAsync( ISqlCallContext ctx, int resId );

    /// <summary>
    /// Destroys a root resource and/or its children thanks to its name.
    /// Note that if <paramref name="withRoot"/> and <paramref name="withChildren"/> are both false, nothing is done.
    /// If the root name doesn't exist, its children can nevertheless be destroyed.
    /// Setting <paramref name="resNameOnly"/> to false will call CK.sResDestroy, destroying the ResId 
    /// and all its resources. By default, only the resource name is destroyed (this is the safest way).
    /// /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="rootResName">The root resource name to destroy.</param>
    /// <param name="withRoot">Whether the root itself must be destroyed.</param>
    /// <param name="withChildren">Whether the root's children must be destroyed.</param>
    /// <param name="resNameOnly">
    /// Set it it false to call sResDestroy (destroying the whole resources) instead of sResNameDestroy.
    /// This has be set explicitely.
    /// </param>
    /// <returns>The awaitable.</returns>
    [SqlProcedure( "sResDestroyByResName" )]
    public abstract Task DestroyByResNameAsync( ISqlCallContext ctx, string rootResName, bool withRoot = true, bool withChildren = true, bool resNameOnly = true );

}
