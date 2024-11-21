using CK.Core;
using CK.SqlServer;
using System.Threading.Tasks;

namespace CK.DB.Group.SimpleNaming;

/// <summary>
/// This package adds a unique GroupName column to the <see cref="Actor.GroupTable"/> table.
/// Group names are automatically numbered on clashes up to the <see cref="MaxClashNumber"/>
/// limit.
/// </summary>
[SqlPackage( ResourcePath = "Res", Schema = "CK" )]
[Versions( "1.0.0,1.0.1,1.0.2" )]
[SqlObjectItem( "transform:vGroup" )]
public abstract partial class Package : SqlPackage
{
    void StObjConstruct( Actor.Package actorPackage )
    {
    }

    /// <summary>
    /// Gets the maximum number of tries before giving up.
    /// This is the maximum " (number)" suffix that can be appended.
    /// </summary>
    public int MaxClashNumber { get; } = 99;

    /// <summary>
    /// Checks a new group name for a given group by returning it unchanged
    /// or an automatically suffixed version " (1)", " (2)", etc. if name already exists
    /// up to <see cref="MaxClashNumber"/>.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="groupId">The group identifier.</param>
    /// <param name="groupName">The new group name.</param>
    /// <returns>The group name or an automatically suffixed version.</returns>
    [SqlScalarFunction( "fGroupGroupNameComputeUnique" )]
    public abstract Task<string> CheckUniqueNameAsync( ISqlCallContext ctx, int groupId, string groupName );

    /// <summary>
    /// Checks a new group name by returning it unchanged
    /// or an automatically suffixed version " (1)", " (2)", etc. if name already exists
    /// up to <see cref="MaxClashNumber"/>.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="groupName">A new group name.</param>
    /// <returns>The group name or an automatically suffixed version.</returns>
    public Task<string> CheckUniqueNameForNewGroupAsync( ISqlCallContext ctx, string groupName ) => CheckUniqueNameAsync( ctx, -1, groupName );

    /// <summary>
    /// Renames a group.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The current actor.</param>
    /// <param name="groupId">The group identifier.</param>
    /// <param name="groupName">The new group name.</param>
    /// <returns>The group name or an automatically suffixed version that has been set.</returns>
    [SqlProcedure( "sGroupGroupNameSet" )]
    public abstract Task<string> GroupRenameAsync( ISqlCallContext ctx, int actorId, int groupId, string groupName );

}
