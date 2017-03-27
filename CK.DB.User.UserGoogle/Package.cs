using CK.Core;
using CK.DB.Auth;
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
    [Versions("1.0.0")]
    [SqlObjectItem( "transform:vUserAuthProvider" )]
    public class Package : SqlPackage
    {
        /// <summary>
        /// Google api url (https://www.googleapis.com).
        /// </summary>
        public static readonly string ApiUrl = "https://www.googleapis.com";

        /// <summary>
        /// Google token endpoint (/oauth2/v4/token).
        /// </summary>
        public static readonly string TokenEndpoint = "/oauth2/v4/token";

        HttpClient _client;

        void Construct( Actor.Package actorPackage, Auth.Package authPackage )
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
        /// Attempts to refreshes the user access token.
        /// On success, the database is updated.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="user">
        /// The user must not be null and <see cref="UserGoogleInfoExtensions.IsValidForRefreshAccessToken"/> must be true.
        /// </param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True on success, false on error.</returns>
        public async Task<bool> RefreshAccessTokenAsync( ISqlCallContext ctx, IUserGoogleInfo user, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            if( ctx == null ) throw new ArgumentNullException( nameof( ctx ) );
            if( user == null ) throw new ArgumentNullException( nameof( user ) );
            if( !user.IsValidForRefreshAccessToken() ) throw new ArgumentException( "User info is not valid." );
            try
            {
                var c = _client ?? (_client = CreateHttpClient( ApiUrl ));
                var parameters = new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "refresh_token", user.RefreshToken },
                    { "client_id", ClientId },
                    { "client_secret", ClientSecret },
                };
                var response = await c.PostAsync( TokenEndpoint, new FormUrlEncodedContent( parameters ), cancellationToken ).ConfigureAwait( false );
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
                    using( ctx.Monitor.OpenError().Send( $"Unable to refresh token for GoogleAccountd = {user.GoogleAccountId}." ) )
                    {
                        ctx.Monitor.Trace().Send( $"Status: {response.StatusCode}, Reason: {response.ReasonPhrase}" );
                        ctx.Monitor.Trace().Send( content );
                    }
                    return false;
                }
                user.AccessToken = (string)token.Single( kv => kv.Key == "access_token" ).Value;
                double exp = (double)token.FirstOrDefault( kv => kv.Key == "expires_in" ).Value;
                user.AccessTokenExpirationTime = exp != 0 ? (DateTime?)DateTime.UtcNow.AddSeconds( exp ) : null;
                // Creates or updates the user (ignoring the created/updated returned value).
                await UserGoogleTable.CreateOrUpdateGoogleUserAsync( ctx, 1, 0, user, CreateOrUpdateMode.UpdateOnly, cancellationToken ).ConfigureAwait( false );
            }
            catch( Exception ex )
            {
                ctx.Monitor.Error().Send( ex, $"Unable to refresh token for GoogleAccountd = {user.GoogleAccountId}." );
                return false;
            }
            return true;
        }

    }
}
