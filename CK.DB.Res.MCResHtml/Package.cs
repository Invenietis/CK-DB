using CK.Core;

namespace CK.DB.Res.MCResHtml
{
    /// <summary>
    /// Package that brings in html value (type is nvarchar(max)) for resources and cultures. 
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
        /// Gets the tMCResHtml table from this package.
        /// </summary>
        [InjectObject]
        public MCResHtmlTable MCResHtmlTable { get; protected set; }
    }
}
