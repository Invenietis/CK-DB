using CK.Core;
using CK.SqlServer;

namespace CK.DB.Res.MCResString;

public abstract partial class MCResStringTable : SqlTable
{
    /// <summary>
    /// Sets a resource string in a given culture. 
    /// When <paramref name="value"/> is null, this removes the associated string.
    /// This can be called with an actual culture (LCID) or a XLCID: the low word (primary LCID) is used.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="resId">The resource identifier.</param>
    /// <param name="lcid">The culture identifier (can be an extended culture identifier: the primary culture is used).</param>
    /// <param name="value">The new string value.</param>
    [SqlProcedure( "sMCResStringSet" )]
    public abstract void SetString( ISqlCallContext ctx, int resId, int lcid, string value );

}
