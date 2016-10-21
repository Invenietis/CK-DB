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
    public abstract partial class ResHtmlTable : SqlTable
    {
        /// <summary>
        /// Sets a resource html value. When <paramref name="value"/> is null, this removes the
        /// associated text.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="resId">The resource identifier.</param>
        /// <param name="value">The new html string value.</param>
        [SqlProcedure( "sResHtmlSet" )]
        public abstract void SetHtml( ISqlCallContext ctx, int resId, string value );

    }
}
