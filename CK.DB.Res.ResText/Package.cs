using CK.Core;

namespace CK.DB.Res.ResText
{
    /// <summary>
    /// Package that brings in text (of type nvarchar(max)) for resources. 
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
        /// Gets the text holder table.
        /// </summary>
        [InjectObject]
        public ResTextTable ResTextTable { get; protected set; }
    }
}
