using CK.Core;

namespace CK.DB.Res.ResName;

/// <summary>
/// This package brings the resource name support (a path-based hierarchy).
/// </summary>
[SqlPackage( Schema = "CK", ResourcePath = "Res" )]
// The 1.0.0 version of the package removes the previous (and crappy) sResDestroyByResNamePrefix, sResDestroyResNameChildren 
// and sResDestroyWithResNameChildren procedures.
[Versions( "1.0.0" )]
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
    /// Gets the CK.tResName table.
    /// </summary>
    [InjectObject]
    public ResNameTable ResNameTable { get; protected set; }
}
