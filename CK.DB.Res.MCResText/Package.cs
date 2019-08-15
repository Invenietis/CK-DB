using CK.Core;

namespace CK.DB.Res.MCResText
{
    /// <summary>
    /// This package brings culture support (XLCID and LCID).
    /// </summary>
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    public class Package : SqlPackage
    {
        ResTable _resTable;
        Culture.Package _culture;

        void StObjConstruct( ResTable resTable, Culture.Package culture )
        {
            _resTable = resTable;
            _culture = culture;
        }

        /// <summary>
        /// Gets the resource table (tRes).
        /// </summary>
        public ResTable ResTable => _resTable;

        /// <summary>
        /// Gets the Culture package.
        /// </summary>
        public Culture.Package Culture => _culture;

        /// <summary>
        /// Gets the tMCResText table from this package.
        /// </summary>
        [InjectObject]
        public MCResTextTable MCResTextTable { get; protected set; }
    }
}
