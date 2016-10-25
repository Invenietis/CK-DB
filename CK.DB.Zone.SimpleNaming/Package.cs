using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Zone.SimpleNaming
{
    /// <summary>
    /// Zone naming makes the CK.tGroup.GroupName unique per zone.
    /// </summary>
    [SqlPackage( ResourceType = typeof(Package) )]
    [Versions( "1.0.0" )]
    [SqlObjectItem( "transform:sGroupMove" )]
    public abstract partial class Package : Group.SimpleNaming.Package
    {
        void Construct( Zone.Package zonePackage )
        {
        }

        /// <summary>
        /// This method is protected: the public CheckUniqueNameForNewGroup exposes it
        /// with the zone identifier.
        /// The inherited CheckUniqueName( ISqlCallContext ctx, int groupId, string groupName ) does
        /// the job by calling the function with its default parameter: function will consider the 
        /// actual zone of the group.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="groupName">The new group name.</param>
        /// <param name="zoneId">The zone should only be provided by <see cref="CheckUniqueNameForNewGroup(ISqlCallContext,int,string)"/>.</param>
        /// <returns>The group name or an automatically suffixed version.</returns>
        [SqlScalarFunction( "transform:fGroupGroupNameComputeUnique" )]
        protected abstract Task<string> CheckUniqueNameAsync( ISqlCallContext ctx, int groupId, string groupName, int zoneId );

        /// <summary>
        /// Checks a new group name in a Zone by returning it unchanged
        /// or an automatically suffixed version " (1)", " (2)", etc. if name already exists
        /// up to <see cref="Group.SimpleNaming.Package.MaxClashNumber"/>.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="zoneId">The zone identifier (the parent) of the future group.</param>
        /// <param name="groupName">A new group name.</param>
        /// <returns>The group name or an automatically suffixed version.</returns>
        public Task<string> CheckUniqueNameForNewGroupAsync( ISqlCallContext ctx, int zoneId, string groupName ) => CheckUniqueNameAsync( ctx, -1, groupName, zoneId );

    }
}
