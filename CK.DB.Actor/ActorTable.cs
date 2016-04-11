using CK.Setup;
using CK.SqlServer.Setup;

namespace CK.DB.Actor
{
    [SqlTable( "tActor", Package = typeof( Package ) ), Versions( "5.0.0" )]
    [SqlObjectItem( "sActorCreate" )]
    public abstract class ActorTable : SqlTable
    {
    }
}
