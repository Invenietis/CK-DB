using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Res.MCResString
{
    /// <summary>
    /// This table holds nvarchar(400) value for a culture and a resource.
    /// </summary>
    [SqlTable( "tMCResString", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    [SqlObjectItem( "transform:sResDestroy, transform:sCultureDestroy" )]
    [SqlObjectItem( "vMCResString" )]
    public abstract partial class MCResStringTable : SqlTable
    {
        /// <summary>
        /// Gets the resource table.
        /// </summary>
        [InjectContract]
        public ResTable ResTable { get; protected set; }

        /// <summary>
        /// Gets the Culture Package.
        /// </summary>
        [InjectContract]
        public Culture.Package Culture { get; protected set; }

        /// <summary>
        /// Sets a resource string in a given culture. 
        /// When <paramref name="value"/> is null, this removes the associated string.
        /// This can be called with an actual culture (LCID) or a XLCID: the low word (primary LCID) is used.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="resId">The resource identifier.</param>
        /// <param name="lcid">The culture identifier (can be an extended culture identifier: the primary culture is used).</param>
        /// <param name="value">The new string value.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sMCResStringSet" )]
        public abstract Task SetStringAsync( ISqlCallContext ctx, int resId, int lcid, string value );

    }
}
