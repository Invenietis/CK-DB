using CK.Core;

namespace CK.DB.Res;

/// <summary>
/// This package brings in the fundamental resource table.
/// </summary>
[SqlPackage( Schema = "CK", ResourcePath = "Res" )]
public class Package : SqlPackage
{
    /// <summary>
    /// Gets the CK.tRes table from this package.
    /// </summary>
    [InjectObject]
    public ResTable ResTable { get; protected set; }
}
