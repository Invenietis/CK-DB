using CK.Core;

namespace CK.DB.Res.ResHtml;

/// <summary>
/// Package that brings in html value (type is nvarchar(max)) for resources. 
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
    /// Gets the html text holder table.
    /// </summary>
    [InjectObject]
    public ResHtmlTable ResHtmlTable { get; protected set; }
}
