using CK.Core;

namespace CK.DB.Res.ResString;

/// <summary>
/// Package that brings in string (of type nvarchar(400)) for resources. 
/// </summary>
[SqlPackage( Schema = "CK", ResourcePath = "Res" )]
public class Package : SqlPackage
{
    void StObjConstruct( Res.Package resource )
    {
    }

    /// <summary>
    /// Gets the resource table.
    /// </summary>
    [InjectObject]
    public ResTable ResTable { get; protected set; }

    /// <summary>
    /// Gets the string holder table.
    /// </summary>
    [InjectObject]
    public ResStringTable ResStringTable { get; protected set; }
}
