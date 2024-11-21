using CK.Core;
using CK.SqlServer;

namespace CK.DB.Res.ResHtml;

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
