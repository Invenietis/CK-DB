using CK.Core;

namespace CK.DB.Res.ResName
{
    /// <summary>
    /// This package brings the resource name support (a path-based hierarchy).
    /// </summary>
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    public class Package : SqlPackage
    {
        void StObjConstruct( Res.Package resource )
        {
        }

        /// <summary>
        /// Gets the resource table.
        /// </summary>
        [InjectObject]
        public ResTable ResTable { get; protected set; }

        /// <summary>
        /// Gets the CK.tResName table.
        /// </summary>
        [InjectObject]
        public ResNameTable ResNameTable { get; protected set; }
    }
}
