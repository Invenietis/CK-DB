using CK.Setup;
using CK.SqlServer.Setup;

namespace CK.DB.Zone.HZone
{
    [SqlView( "vZone_AllChildren", Package = typeof( Package ) )]
    //[Requires( "CK.vZone" )]
    public abstract class ZoneAllChildrenView : SqlView
    {

        void Construct( ZoneTable zone )
        {
        }
    }
}
