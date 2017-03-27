using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Res.ResString
{
    /// <summary>
    /// This table holds nvarchar(400) value for a resource.
    /// </summary>
    [SqlTable( "tResString", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    [SqlObjectItem( "transform:sResDestroy" )]
    public abstract partial class ResStringTable : SqlTable
    {
        /// <summary>
        /// Gets the resource table.
        /// </summary>
        [InjectContract]
        public ResTable ResTable { get; protected set; }

        /// <summary>
        /// Sets a resource string. When <paramref name="value"/> is null, this removes the
        /// associated string.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="resId">The resource identifier.</param>
        /// <param name="value">The new string value.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sResStringSet" )]
        public abstract Task SetStringAsync( ISqlCallContext ctx, int resId, string value );

    }
}
