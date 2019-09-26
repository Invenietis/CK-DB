using CK.Core;

namespace CK.DB.Auth.AuthScope
{
    /// <summary>
    /// Holds the scope name.
    /// </summary>
    [SqlTable("tAuthScope", Package = typeof(Package))]
    [Versions( "1.0.0,1.0.1")]
    public class AuthScopeTable : SqlTable
    {
    }
}
