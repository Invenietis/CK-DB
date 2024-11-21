using CK.Core;
using CK.SqlServer;

namespace CK.DB.Res.ResString;

public abstract partial class ResStringTable : SqlTable
{
    /// <summary>
    /// Sets a resource string. When <paramref name="value"/> is null, this removes the
    /// associated string.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="resId">The resource identifier.</param>
    /// <param name="value">The new string value.</param>
    [SqlProcedure( "sResStringSet" )]
    public abstract void SetString( ISqlCallContext ctx, int resId, string value );

}
