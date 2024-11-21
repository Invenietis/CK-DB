using CK.Core;

namespace CK.DB.Actor;

/// <summary>
/// Fundamental table for Actor: the base entity of <see cref="GroupTable"/> and <see cref="UserTable"/>.
/// </summary>
[SqlTable( "tActor", Package = typeof( Package ) )]
[Versions( "5.0.0,5.0.1" )]
[SqlObjectItem( "sActorCreate" )]
public abstract class ActorTable : SqlTable
{
}
