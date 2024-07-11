using System;
using System.Threading.Tasks;
using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using NUnit.Framework;
using CK.DB.Auth;
using CK.DB.Auth.AuthScope;
using FluentAssertions;
using static CK.Testing.MonitorTestHelper;
using System.Diagnostics;

namespace CK.DB.User.UserGoogle.AuthScope.Tests
{
    [TestFixture]
    public class UserGoogleAuthScopeTests
    {

        [Test]
        public async Task non_user_google_ScopeSet_is_null_Async()
        {
            var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
            var p = SharedEngine.Map.StObjs.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var id = await user.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );
                (await p.ReadScopeSetAsync( ctx, id )).Should().BeNull();
            }
        }

        [Test]
        public async Task setting_default_scopes_impact_new_users_Async()
        {
            var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
            var p = SharedEngine.Map.StObjs.Obtain<Package>();
            var factory = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserGoogleInfo>>();
            using( var ctx = new SqlStandardCallContext() )
            {
                AuthScopeSet original = await p.ReadDefaultScopeSetAsync( ctx );
                original.Contains( "nimp" ).Should().BeFalse();
                original.Contains( "thing" ).Should().BeFalse();
                original.Contains( "other" ).Should().BeFalse();

                {
                    int id = await user.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );
                    IUserGoogleInfo userInfo = factory.Create();
                    userInfo.GoogleAccountId = Guid.NewGuid().ToString();
                    await p.UserGoogleTable.CreateOrUpdateGoogleUserAsync( ctx, 1, id, userInfo );
                    var info = await p.UserGoogleTable.FindKnownUserInfoAsync( ctx, userInfo.GoogleAccountId );
                    Debug.Assert( info != null );
                    AuthScopeSet userSet = await p.ReadScopeSetAsync( ctx, info.UserId );
                    userSet.ToString().Should().Be( original.ToString() );
                }
                AuthScopeSet replaced = original.Clone();
                replaced.Add( new AuthScopeItem( "nimp" ) );
                replaced.Add( new AuthScopeItem( "thing", ScopeWARStatus.Rejected ) );
                replaced.Add( new AuthScopeItem( "other", ScopeWARStatus.Accepted ) );
                await p.AuthScopeSetTable.SetScopesAsync( ctx, 1, replaced );
                var readback = await p.ReadDefaultScopeSetAsync( ctx );
                readback.ToString().Should().Be( replaced.ToString() );
                // Default scopes have non W status!
                // This must not impact new users: their satus must always be be W.
                readback.ToString().Should().Contain( "[R]thing" )
                                            .And.Contain( "[A]other" );

                {
                    int id = await user.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );
                    IUserGoogleInfo userInfo = p.UserGoogleTable.CreateUserInfo<IUserGoogleInfo>();
                    userInfo.GoogleAccountId = Guid.NewGuid().ToString();
                    await p.UserGoogleTable.CreateOrUpdateGoogleUserAsync( ctx, 1, id, userInfo, UCLMode.CreateOnly | UCLMode.UpdateOnly );
                    userInfo = (IUserGoogleInfo)(await p.UserGoogleTable.FindKnownUserInfoAsync( ctx, userInfo.GoogleAccountId ))!.Info;
                    AuthScopeSet userSet = await p.ReadScopeSetAsync( ctx, id );
                    userSet.ToString().Should().Contain( "[W]thing" )
                                               .And.Contain( "[W]other" )
                                               .And.Contain( "[W]nimp" );
                }
                await p.AuthScopeSetTable.SetScopesAsync( ctx, 1, original );
            }
        }

    }

}

