using CK.Core;
using CK.SqlServer;

namespace CK.DB.Group.SimpleNaming;

public abstract partial class Package : SqlPackage
{

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
    public abstract string CheckUniqueName( ISqlCallContext ctx, int groupId, string groupName );

    /// <summary>
    /// Checks a new group name by returning it unchanged
    /// or an automatically suffixed version " (1)", " (2)", etc. if name already exists
    /// up to <see cref="MaxClashNumber"/>.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="groupName">A new group name.</param>
    /// <returns>The group name or an automatically suffixed version.</returns>
    public string CheckUniqueNameForNewGroup( ISqlCallContext ctx, string groupName ) => CheckUniqueName( ctx, -1, groupName );

    /// <summary>
    /// Renames a group.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The current actor.</param>
    /// <param name="groupId">The group identifier.</param>
    /// <param name="groupName">The new group name.</param>
    /// <returns>The group name or an automatically suffixed version that has been set.</returns>
    [SqlProcedure( "sGroupGroupNameSet" )]
    public abstract string GroupRename( ISqlCallContext ctx, int actorId, int groupId, string groupName );

}
