using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Culture
{
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
        [SqlProcedure( "sCultureRegister" )]
        public abstract void Register( ISqlCallContext ctx, int lcid, string name, string englishName, string nativeName );

        /// <summary>
        /// Destroys a Culture, be it an actual culture (LCID) or a pure XLCID. 
        /// When destroying a LCID, all XLCID that have the LCID as their primary culture are also destroyed.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="xlcid">The extended culture identifier to destroy.</param>
        [SqlProcedure( "sCultureDestroy" )]
        public abstract void DestroyCulture( ISqlCallContext ctx, int xlcid );

        /// <summary>
        /// Sets the fallback of a LCID.
        /// The list must start with the <paramref name="lcid"/> itself otherwise an error is raised.
        /// The list do not not need to be complete: existing LCID with their current ordering
        /// will be automatically added.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="lcid">The culture identifier.</param>
        /// <param name="fallbacks">The fallbacks that must start with the lcid.</param>
        public void SetLCIDFallbaks( ISqlCallContext ctx, int lcid, IEnumerable<int> fallbacks )
        {
            SetLCIDFallbaks( ctx, lcid, string.Join( ",", fallbacks ) );
        }

        /// <summary>
        /// Protected.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="lcid"></param>
        /// <param name="fallbacksLCID"></param>
        [SqlProcedure( "sCultureFallbackSet" )]
        internal protected abstract void SetLCIDFallbaks( ISqlCallContext ctx, int lcid, string fallbacksLCID );

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
        public int AssumeXLCID( ISqlCallContext ctx, IEnumerable<int> fallbacks, bool allowLCIDMapping = false )
        {
            return AssumeXLCID( ctx, string.Join( ",", fallbacks ), allowLCIDMapping );
        }

        /// <summary>
        /// Protected method: <see cref="AssumeXLCIDAsync(ISqlCallContext, IEnumerable{int}, bool)"/> does the job.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="fallbacksLCID"></param>
        /// <param name="allowLCIDMapping"></param>
        /// <returns></returns>
        [SqlProcedure( "sCultureAssumeXLCID" )]
        internal protected abstract int AssumeXLCID( ISqlCallContext ctx, string fallbacksLCID, bool allowLCIDMapping );

        /// <summary>
        /// Gets the <see cref="CultureData"/> for a given culture identifier.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="lcid">The culture identifier (between 0 and 0sFFFF).</param>
        /// <returns>The culture data or null.</returns>
        public CultureData GetCulture( ISqlCallContext ctx, int lcid )
        {
            using( var c = new SqlCommand( $"select Name, EnglishName, NativeName from CK.tLCID where LCID=@LCID" ) )
            {
                c.Parameters.AddWithValue( "@LCID", lcid );
                var p = ctx.Executor.GetProvider( Database.ConnectionString );
                object[] data = p.ReadFirstRow( c );
                return data == null ? null : new CultureData( lcid, (string)data[0], (string)data[1], (string)data[2] );
            }
        }


        /// <summary>
        /// Gets the <see cref="ExtendedCultureData"/> for a given extended culture identifier.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="xlcid">The extended culture identifier (greater than 0 and not equal to 0xFFFF).</param>
        /// <returns>The extended culture data or null.</returns>
        public ExtendedCultureData GetExtendedCulture( ISqlCallContext ctx, int xlcid )
        {
            using( var c = new SqlCommand( $"select Fallbacks from CK.vXLCID where XLCID=@XLCID" ) )
            {
                c.Parameters.AddWithValue( "@XLCID", xlcid );
                var p = ctx.Executor.GetProvider( Database.ConnectionString );
                string s = (string)p.ExecuteScalar( c );
                if( s == null ) return null;
                string[] f = s.Split( CultureData._separators );
                CultureData[] d = new CultureData[f.Length / 4];
                int idx = 0;
                for( int i = 0; i < d.Length; ++i )
                {
                    d[i] = new CultureData( int.Parse( f[idx++] ), f[idx++], f[idx++], f[idx++] );
                }
                return new ExtendedCultureData( xlcid, d );
            }
        }
    }
}
