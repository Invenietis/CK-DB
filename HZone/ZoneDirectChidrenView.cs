using CK.SqlServer.Setup;

namespace CK.DB.Zone.HZone
{
    [SqlView( "vZone_DirectChildren", Package = typeof( Package ) )]
    public abstract class ZoneDirectChildrenView : SqlView
    {
        void Construct( ZoneAllChildrenView vZoneAll )
        {
        }
    }
}
