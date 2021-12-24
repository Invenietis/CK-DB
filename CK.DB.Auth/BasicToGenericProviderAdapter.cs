using CK.Core;
using CK.SqlServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            if( payload is ValueTuple<int, string> vt1 ) return Tuple.Create( vt1.Item1, vt1.Item2 );
            if( payload is ValueTuple<string, string> vt2 ) return Tuple.Create( vt2.Item1, vt2.Item2 );
            var kindOfDic = PocoFactoryExtensions.ToValueTuples( payload );
            if( kindOfDic != null )
            {
                int? userId = null;
                string userName = null;
                string password = null;
                foreach( var kv in kindOfDic )
                {
                    if( StringComparer.OrdinalIgnoreCase.Equals( kv.Key, "UserId" ) )
                    {
                        userId = kv.Value as int?;
                        if( !userId.HasValue && kv.Value is double d )
                        {
                            userId = (int)d;
                        }
                        if( !userId.HasValue && kv.Value is string s )
                        {
                            if( Int32.TryParse( s, out int id ) ) userId = id;
                        }
                    }
                    if( StringComparer.OrdinalIgnoreCase.Equals( kv.Key, "UserName" ) ) userName = kv.Value as string;
                    if( StringComparer.OrdinalIgnoreCase.Equals( kv.Key, "Password" ) ) password = kv.Value as string;
                    if( password != null && (userId != null || userName != null) ) break;
                }
                if( password != null )
                {
                    if( userName != null ) return Tuple.Create( userName, password );
                    if( userId.HasValue ) return Tuple.Create( userId.Value, password );
                    Throw.ArgumentException( nameof( payload ), "Invalid payload. Missing 'UserId' -> int or 'UserName' -> string entry." );
                }
                Throw.ArgumentException( nameof( payload ), "Invalid payload. Missing 'Password' -> string entry." );
            }
            Throw.ArgumentException( nameof( payload ), "Invalid payload. It must be either a Tuple or ValueTuple (int,string) or (string,string) or a IDictionary<string,object?> or IEnumerable<KeyValuePair<string,object?>> or IEnumerable<(string,object?)> with 'Password' -> string and 'UserId' -> int or 'UserName' -> string entries." );
            // Wtf? Throw.ArgumentException is [DoesNotReturn] but this is ignored by Roslyn here :(
            return null;
        }

    }
}
