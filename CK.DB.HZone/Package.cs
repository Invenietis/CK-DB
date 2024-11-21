using CK.Core;

namespace CK.DB.HZone;

/// <summary>
/// This package adds the support for hierarchical zones based on the Sql Server hierarchyid type.
/// </summary>
[SqlPackage( ResourcePath = "Res", ResourceType = typeof( Package ) )]
[Versions( "1.0.0" )]
[SqlObjectItem( "transform:sGroupUserAdd, transform:sGroupMove, transform:vZone" )]
[SqlObjectItem( "vZoneDirectChildren, vZoneAllChildren" )]
public abstract class Package : Zone.Package
{
    void StObjConstruct( Zone.Package zone )
    {
    }

    /// <summary>
    /// Gets the Zone table that is extended by this package.
    /// </summary>
    public new ZoneTable ZoneTable => (ZoneTable)base.ZoneTable;
}
