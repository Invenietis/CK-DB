using CK.Core;

namespace CK.DB.Actor
{
    /// <summary>
    /// Basic packages with Actors, Users and Groups.
    /// </summary>
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    [Versions( "5.0.0" )]
    public abstract class Package : SqlPackage
    {
        /// <summary>
        /// Gets the GroupTable.
        /// </summary>
        [InjectObject]
        public GroupTable GroupTable { get; protected set; }

        /// <summary>
        /// Gets the UserTable.
        /// </summary>
        [InjectObject]
        public UserTable UserTable { get; protected set; }

    }
}
