using CK.Core;
using CK.SqlServer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CK.DB.Culture;

/// <summary>
/// Culture package.
/// Currently, no data is cached by this implementation: eventually <see cref="CultureData"/> and <see cref="ExtendedCultureData"/>
/// should be cached.
/// </summary>
[SqlPackage( Schema = "CK", ResourcePath = "Res" )]
public abstract partial class Package : SqlPackage
{
    /// <summary>
    /// Updates or creates an actual culture.
    /// When creating a new culture, its fallbacks are by default the english ones
    /// and this newcomer is added as the last fallback to any existing XLCID.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="lcid">The culture identifier: must be between 0 and 0xFFFF.</param>
    /// <param name="name">Standard name of the culture: "az-Cyrl".</param>
    /// <param name="englishName">Name of the culture in english: "Azerbaijani (Cyrillic)".</param>
    /// <param name="nativeName">Name of the culture in the culture iself: "Азәрбајҹан дили".</param>
    /// <returns>The awaitable.</returns>
    public Task RegisterAsync( ISqlCallContext ctx, int lcid, string name, string englishName, string nativeName )
    {
        CultureData.ValidateParameters( lcid, name, englishName, nativeName );
        return DoRegisterAsync( ctx, lcid, name, englishName, nativeName );
    }

    [SqlProcedure( "sCultureRegister" )]
    protected abstract Task DoRegisterAsync( ISqlCallContext ctx, int lcid, string name, string englishName, string nativeName );

    /// <summary>
    /// Destroys a Culture, be it an actual culture (LCID) or a pure XLCID. 
    /// When destroying a LCID, all XLCID that have the LCID as their primary culture are also destroyed.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="xlcid">The extended culture identifier to destroy.</param>
    /// <returns>The awaitable.</returns>
    [SqlProcedure( "sCultureDestroy" )]
    public abstract Task DestroyCultureAsync( ISqlCallContext ctx, int xlcid );

    /// <summary>
    /// Sets the fallback of a LCID.
    /// The list must start with the <paramref name="lcid"/> itself otherwise an error is raised.
    /// The list do not not need to be complete: existing LCID with their current ordering
    /// will be automatically added.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="lcid">The culture identifier.</param>
    /// <param name="fallbacks">The fallbacks that must start with the lcid.</param>
    /// <returns>The awaitable.</returns>
    public Task SetLCIDFallbaksAsync( ISqlCallContext ctx, int lcid, IEnumerable<int> fallbacks )
    {
        return SetLCIDFallbaksAsync( ctx, lcid, string.Join( ",", fallbacks ) );
    }

    /// <summary>
    /// Protected method.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="lcid"></param>
    /// <param name="fallbacksLCID"></param>
    /// <returns></returns>
    [SqlProcedure( "sCultureFallbackSet" )]
    internal protected abstract Task SetLCIDFallbaksAsync( ISqlCallContext ctx, int lcid, string fallbacksLCID );

    /// <summary>
    /// Finds or creates a XLCID with a specified fallbacks chain.
    /// The list do not not need to be complete (the smaller it is, the more chances there are
    /// to reuse an existing fallback instead of creating a new one): LCID with their current ordering
    /// will be automatically added based on the PrimaryLCID fallbacks.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="fallbacks">The fallbacks to find or create.</param>
    /// <param name="allowLCIDMapping">
    /// When true, a LCID may be returned if its current fallbacks satisfy the request.
    /// Since fallbacks of a LCID can be changed, this is not guaranteed to be stable.
    /// By default, a pure XLCID is obtained or created: it is immutable (as long as registered
    /// cultures do not change) and will be destroyed only if its primary LCID is destroyed.
    /// </param>
    /// <returns>A XLCID with the specified fallbacks.</returns>
    public Task<int> AssumeXLCIDAsync( ISqlCallContext ctx, IEnumerable<int> fallbacks, bool allowLCIDMapping = false )
    {
        return AssumeXLCIDAsync( ctx, string.Join( ",", fallbacks ), allowLCIDMapping );
    }

    /// <summary>
    /// Protected method: <see cref="AssumeXLCIDAsync(ISqlCallContext, IEnumerable{int}, bool)"/> does the job.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="fallbacksLCID"></param>
    /// <param name="allowLCIDMapping"></param>
    /// <returns></returns>
    [SqlProcedure( "sCultureAssumeXLCID" )]
    internal protected abstract Task<int> AssumeXLCIDAsync( ISqlCallContext ctx, string fallbacksLCID, bool allowLCIDMapping );

}
