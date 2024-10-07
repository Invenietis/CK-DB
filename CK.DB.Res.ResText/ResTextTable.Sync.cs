using CK.Core;
using CK.SqlServer;

namespace CK.DB.Res.ResText;

public abstract partial class ResTextTable : SqlTable
{
    /// <summary>
    /// Sets a resource text. When <paramref name="value"/> is null, this removes the
    /// associated string.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="resId">The resource identifier.</param>
    /// <param name="value">The new text value. Null to remove it.</param>
    [SqlProcedure( "sResTextSet" )]
    public abstract void SetText( ISqlCallContext ctx, int resId, string value );


}
