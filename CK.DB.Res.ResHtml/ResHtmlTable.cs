using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Res.ResHtml
{
    /// <summary>
    /// This table holds nvarchar(max) value that must contain html text for a resource.
    /// </summary>
    [SqlTable( "tResHtml", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    [SqlObjectItem( "transform:sResDestroy" )]
    public abstract partial class ResHtmlTable : SqlTable
    {
        /// <summary>
        /// Gets the resource table.
        /// </summary>
        [InjectContract]
        public ResTable ResTable { get; protected set; }

        /// <summary>
        /// Sets a resource html value. When <paramref name="value"/> is null, this removes the
        /// associated text.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="resId">The resource identifier.</param>
        /// <param name="value">The new html string value.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sResHtmlSet" )]
        public abstract Task SetHtmlAsync( ISqlCallContext ctx, int resId, string value );

    }
}
