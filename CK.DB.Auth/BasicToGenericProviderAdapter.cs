using CK.SqlServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CK.DB.Auth
{
    class BasicToGenericProviderAdapter : IGenericAuthenticationProvider
    {
        readonly IBasicAuthenticationProvider _basic;
        static public readonly string Name = "Basic";

        public BasicToGenericProviderAdapter( IBasicAuthenticationProvider basic )
        {
            Debug.Assert( basic != null );
            _basic = basic;
        }

        string IGenericAuthenticationProvider.ProviderName => Name;

        UCLResult IGenericAuthenticationProvider.CreateOrUpdateUser( ISqlCallContext ctx, int actorId, int userId, object payload, UCLMode mode )
        {
            string password = payload as string;
            if( password == null ) throw new ArgumentException( "Must be a string (the password).", nameof( payload ) );
            return _basic.CreateOrUpdatePasswordUser( ctx, actorId, userId, password, mode );
        }

        Task<UCLResult> IGenericAuthenticationProvider.CreateOrUpdateUserAsync( ISqlCallContext ctx, int actorId, int userId, object payload, UCLMode mode, CancellationToken cancellationToken )
        {
            string password = payload as string;
            if( password == null ) throw new ArgumentException( "Must be a string (the password).", nameof( payload ) );
            return _basic.CreateOrUpdatePasswordUserAsync( ctx, actorId, userId, password, mode, cancellationToken );
        }

        void IGenericAuthenticationProvider.DestroyUser( ISqlCallContext ctx, int actorId, int userId, string schemeSuffix )
        {
            _basic.DestroyPasswordUser( ctx, actorId, userId );
        }

        Task IGenericAuthenticationProvider.DestroyUserAsync( ISqlCallContext ctx, int actorId, int userId, string schemeSuffix, CancellationToken cancellationToken )
        {
            return _basic.DestroyPasswordUserAsync( ctx, actorId, userId, cancellationToken );
        }

        LoginResult IGenericAuthenticationProvider.LoginUser( ISqlCallContext ctx, object payload, bool actualLogin )
        {
            payload = ExtractPayload( payload );
            Tuple<string, string> byName = payload as Tuple<string, string>;
            if( byName != null ) return _basic.LoginUser( ctx, byName.Item1, byName.Item2, actualLogin );
            var byId = (Tuple<int, string>)payload;
            return _basic.LoginUser( ctx, byId.Item1, byId.Item2, actualLogin );
        }

        async Task<LoginResult> IGenericAuthenticationProvider.LoginUserAsync( ISqlCallContext ctx, object payload, bool actualLogin, CancellationToken cancellationToken )
        {
            payload = ExtractPayload( payload );
            Tuple<string, string> byName = payload as Tuple<string, string>;
            if( byName != null ) return await _basic.LoginUserAsync( ctx, byName.Item1, byName.Item2, actualLogin, cancellationToken );
            Tuple<int, string> byId = (Tuple<int, string>)payload;
            return await _basic.LoginUserAsync( ctx, byId.Item1, byId.Item2, actualLogin );
        }

        static object ExtractPayload( object payload )
        {
            if( payload is Tuple<string, string> ) return payload;
            if( payload is Tuple<int, string> ) return payload;
            var kindOfDic = payload as IEnumerable<KeyValuePair<string, object>>;
            if( kindOfDic != null )
            {
                int? userId = null;
                string userName = null;
                string password = null;
                foreach( var kv in kindOfDic )
                {
                    if( StringComparer.OrdinalIgnoreCase.Equals( kv.Key, "UserId" ) ) userId = kv.Value as int?;
                    if( StringComparer.OrdinalIgnoreCase.Equals( kv.Key, "UserName" ) ) userName = kv.Value as string;
                    if( StringComparer.OrdinalIgnoreCase.Equals( kv.Key, "Password" ) ) password = kv.Value as string;
                    if( password != null && (userId != null || userName != null) ) break;
                }
                if( password != null )
                {
                    if( userName != null ) return Tuple.Create( userName, password );
                    if( userId.HasValue ) return Tuple.Create( userId.Value, password );
                    throw new ArgumentException( "Invalid payload. Missing 'UserId' -> int or 'UserName' -> string entry.", nameof( payload ) );
                }
                throw new ArgumentException( "Invalid payload. Missing 'Password' -> string entry.", nameof( payload ) );
            }
            throw new ArgumentException( "Invalid payload. It must be either a Tuple<int,string>, a Tuple<string,string> or a IDictionary<string,object> or IEnumerable<KeyValuePair<string,object>> with 'Password' -> string and 'UserId' -> int or 'UserName' -> string entries.", nameof( payload ) );
        }

    }
}
