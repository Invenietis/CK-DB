using CK.Core;

namespace CK.DB.Actor
{
    /// <summary>
    /// Internal Actor x Group relation.
    /// </summary>
    [SqlTable( "tActorProfile", Package = typeof( Package ) )]
    [Versions( "5.0.0,5.0.1" )]
    public abstract class ActorProfileTable : SqlTable
    {
        void StObjConstruct( ActorTable actor, GroupTable group )
        {
        }
    }
}
