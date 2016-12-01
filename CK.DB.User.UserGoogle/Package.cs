using CK.Core;
using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;
using CK.Text;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CK.DB.User.UserGoogle
{
    /// <summary>
    /// Package that adds Google authentication support for users. 
    /// </summary>
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    public class Package : SqlPackage
    {
        /// <summary>
        /// Google token endpoint.
        /// </summary>
        public static readonly string TokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";

        HttpClient _client;

        void Construct(Actor.Package resource)
        {
        }

        static HttpClient CreateHttpClient( string baseAddress )
        {
            var c = new HttpClient( new HttpClientHandler() )
            {
                BaseAddress = new Uri( baseAddress )
            };
            c.DefaultRequestHeaders.Accept.Clear();
            c.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
            return c;
        }

        /// <summary>
        /// Gets or sets the Google's application client identifier.
        /// This is required by <see cref="RefreshAccessTokenAsync"/>.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the Google's application client secret.
        /// This is required by <see cref="RefreshAccessTokenAsync"/>.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets the user Google table.
        /// </summary>
        [InjectContract]
        public UserGoogleTable UserGoogleTable { get; protected set; }

        /// <summary>
        /// Gets the default scopes that are required for all users.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The <see cref="SimpleScopes"/> for all users.</returns>
        public async Task<SimpleScopes> GetDefaultScopesAsync( ISqlCallContext ctx, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            return new SimpleScopes( await UserGoogleTable.ScalarByGoogleAccountIdAsync<string>( ctx, "Scopes", string.Empty, cancellationToken ).ConfigureAwait( false ) );
        }

        /// <summary>
        /// Sets the default scopes that are required for all users.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="scopes">The scopes to set (must be valid).</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The awaitable.</returns>
        public async Task SetDefaultScopesAsync( ISqlCallContext ctx, SimpleScopes scopes, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            if( !scopes.IsValid ) throw new ArgumentException( "Scopes must be valid." );
            using( var c = new SqlCommand( $"update CK.tUserGoogle set Scopes=@S where GoogleAccountId=''" ) )
            using( await (c.Connection = ctx[Database]).EnsureOpenAsync().ConfigureAwait( false ) )
            {
                c.Parameters.AddWithValue( "@S", scopes.Scopes );
                await c.ExecuteNonQueryAsync().ConfigureAwait( false );
            }
        }

        /// <summary>
        /// Attempts to refreshes the user access token.
        /// On success, the database is updated.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="monitor">Monitor that will receive error details.</param>
        /// <param name="user">
        /// The user must not be null and <see cref="UserGoogleInfo.IsValid"/> must be true.
        /// </param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns></returns>
        public async Task<bool> RefreshAccessTokenAsync( ISqlCallContext ctx, IActivityMonitor monitor, UserGoogleInfo user, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            if( ctx == null ) throw new ArgumentNullException( nameof( ctx ) );
            if( monitor == null ) throw new ArgumentNullException( nameof( monitor ) );
            if( user == null ) throw new ArgumentNullException( nameof( user ) );
            if( !user.IsValid ) throw new ArgumentException( "User info is not valid." );
            var c = _client ?? (_client = CreateHttpClient( TokenEndpoint ));
            var parameters = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", user.RefreshToken },
                { "client_id", ClientId },
                { "client_secret", ClientSecret },
            };
            var response = await c.PostAsync( string.Empty, new FormUrlEncodedContent( parameters ), cancellationToken ).ConfigureAwait( false );
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait( false );
            List<KeyValuePair<string, object>> token = null;
            if( response.IsSuccessStatusCode )
            {
                var m = new StringMatcher( content );
                object tok;
                if( m.MatchJSONObject( out tok ) ) token = tok as List<KeyValuePair<string, object>>;
            }
            if( token == null )
            {
                using( monitor.OpenError().Send( $"Unable to refresh token for UserId = {user.UserId}." ) )
                {
                    monitor.Trace().Send( $"Status: {response.StatusCode}, Reason: {response.ReasonPhrase}" );
                    monitor.Trace().Send( content );
                }
                return false;
            }
            user.AccessToken = (string)token.Single( kv => kv.Key == "access_token" ).Value;
            double exp = (double)token.FirstOrDefault( kv => kv.Key == "expires_in" ).Value;
            user.AccessTokenExpirationTime = exp != 0 ? (DateTime?)DateTime.UtcNow.AddSeconds( exp ) : null;
            // Creates or updates the user (ignoring the created/updated returned value).
            await UserGoogleTable.CreateOrUpdateGoogleUserAsync( ctx, user.UserId, user, cancellationToken );
            return true;
        }

    }
}
