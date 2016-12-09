using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;
using CK.Text;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Auth.AuthScope
{
    public abstract partial class AuthScopeSetTable
    {
        /// <summary>
        /// Factory method for <see cref="AuthScopeItem"/> (or specialized one).
        /// </summary>
        /// <param name="r">The data reader.</param>
        /// <returns>A new scope.</returns>
        protected virtual AuthScopeItem CreateAuthScope( SqlDataReader r )
        {
            string n = r.GetString( 0 );
            ScopeWARStatus s;
            switch( r.GetSqlChars( 1 ).Buffer[0] )
            {
                case 'A': s = ScopeWARStatus.Accepted; break;
                case 'R': s = ScopeWARStatus.Rejected; break;
                default: s = ScopeWARStatus.Waiting; break;
            }
            return new AuthScopeItem( n, s, r.GetDateTime( 2 ) );
        }

        /// <summary>
        /// Creates the command to read the name, status and last write time of scopes from a scope set selector.
        /// This can be overridden to read more specialized data.
        /// </summary>
        /// <param name="scopeSetIdSelector">The scope set identifier selector.</param>
        /// <returns>The command.</returns>
        public virtual SqlCommand CreateReadCommand( string scopeSetIdSelector )
        {
            return new SqlCommand( $@"{scopeSetIdSelector}; with src( ScopeSetId ) as ({scopeSetIdSelector}) select s.ScopeName, s.WARStatus, s.WARStatusLastWrite from CK.vAuthScopeSetContent s inner join src on src.ScopeSetId = s.ScopeSetId" );
        }

        static string ToString( IEnumerable<AuthScopeItem> scopes )
        {
            return scopes.Where( s => s != null ).OrderBy( s => s.ScopeName ).Select( s => s.ToString() ).Concatenate( " " );
        }
    }
}
