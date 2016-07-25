using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Res.ResText
{
    [SqlTable( "tResText", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    [SqlObjectItem( "transform:sResDestroy" )]
    public abstract partial class ResTextTable : SqlTable
    {
        /// <summary>
        /// Gets the resource table.
        /// </summary>
        [InjectContract]
        public ResTable ResTable { get; protected set; }

        /// <summary>
        /// Sets a resource text. When <param name="value"/> is null, this removes the
        /// associated string.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="resId">The resource identifier.</param>
        /// <param name="value">The new text value. Null to remove it.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sResTextSet" )]
        public abstract Task SetTextAsync( ISqlCallContext ctx, int resId, string value );

        /// <summary>
        /// Sets a resource text. When <param name="value"/> is null, this removes the
        /// associated string.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="resId">The resource identifier.</param>
        /// <param name="value">The new text value. Null to remove it.</param>
        [SqlProcedure( "sResTextSet" )]
        public abstract void SetText( ISqlCallContext ctx, int resId, string value );


    }
}
