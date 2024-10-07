using CK.Core;
using CK.SqlServer;
using System.Threading.Tasks;

namespace CK.DB.Res.ResText;

/// <summary>
/// This table holds nvarchar(max) value for a resource.
/// </summary>
[SqlTable( "tResText", Package = typeof( Package ) )]
[Versions( "1.0.0" )]
[SqlObjectItem( "transform:sResDestroy" )]
public abstract partial class ResTextTable : SqlTable
{
    /// <summary>
    /// Gets the resource table.
    /// </summary>
    [InjectObject]
    public ResTable ResTable { get; protected set; }

    /// <summary>
    /// Sets a resource text. When <paramref name="value"/> is null, this removes the
    /// associated string.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="resId">The resource identifier.</param>
    /// <param name="value">The new text value. Null to remove it.</param>
    /// <returns>The awaitable.</returns>
    [SqlProcedure( "sResTextSet" )]
    public abstract Task SetTextAsync( ISqlCallContext ctx, int resId, string value );

}
