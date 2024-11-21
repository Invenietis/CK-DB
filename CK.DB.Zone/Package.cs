using CK.Core;
using System.Diagnostics.CodeAnalysis;

namespace CK.DB.Zone;

/// <summary>
/// This package subordinates Groups to Zones.
/// </summary>
[SqlPackage( ResourcePath = "Res", ResourceType = typeof( Package ) )]
[Versions( "5.0.0" )]
public abstract class Package : Actor.Package
{
    /// <summary>
    /// Gets the GroupTable that this package extends.
    /// </summary>
    public new GroupTable GroupTable => (GroupTable)base.GroupTable;

    /// <summary>
    /// Gets the GroupTable that this package defines.
    /// </summary>
    [InjectObject, AllowNull]
    public ZoneTable ZoneTable { get; protected set; }

}
