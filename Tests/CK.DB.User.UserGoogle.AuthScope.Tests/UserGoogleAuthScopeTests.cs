using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using NUnit.Framework;
using System.Linq;
using CK.DB.Auth;
using CK.DB.Auth.AuthScope;

namespace CK.DB.User.UserGoogle.AuthScope.Tests
{
    [TestFixture]
    public class UserGoogleAuthScopeTests
    {
        [Test]
        public async Task setting_default_scopes_impact_new_users()
        {
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                AuthScopeSet original = await p.ReadDefaultScopeSetAsync( ctx );
                Assert.That( !original.Contains( "nimp" ) && !original.Contains( "thing" ) && !original.Contains( "other" ) );

                {
                    int id = await user.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );
                    UserGoogleInfo userInfo = new UserGoogleInfo() { UserId = id, GoogleAccountId = Guid.NewGuid().ToString() };
                    await p.UserGoogleTable.CreateOrUpdateGoogleUserAsync( ctx, 1, userInfo );
                    userInfo = await p.UserGoogleTable.FindUserInfoAsync( ctx, userInfo.GoogleAccountId );
                    AuthScopeSet userSet = await p.ReadScopeSetAsync( ctx, userInfo.UserId );
                    Assert.That( userSet.ToString(), Is.EqualTo( original.ToString() ) );
                }
                AuthScopeSet replaced = original.Clone();
                replaced.Add( new AuthScopeItem( "nimp" ) );
                replaced.Add( new AuthScopeItem( "thing", ScopeWARStatus.Rejected ) );
                replaced.Add( new AuthScopeItem( "other", ScopeWARStatus.Accepted ) );
                await p.AuthScopeSetTable.SetScopesAsync( ctx, 1, replaced );
                var readback = await p.ReadDefaultScopeSetAsync( ctx );
                Assert.That( readback.ToString(), Is.EqualTo( replaced.ToString() ) );
                // Default scopes have non W status!
                // This must not impact new users: their satus must always be be W.
                Assert.That( readback.ToString(), Does.Contain( "[R]thing" ) );
                Assert.That( readback.ToString(), Does.Contain( "[A]other" ) );

                {
                    int id = await user.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );
                    UserGoogleInfo userInfo = new UserGoogleInfo() { UserId = id, GoogleAccountId = Guid.NewGuid().ToString() };
                    await p.UserGoogleTable.CreateOrUpdateGoogleUserAsync( ctx, 1, userInfo );
                    userInfo = await p.UserGoogleTable.FindUserInfoAsync( ctx, userInfo.GoogleAccountId );
                    AuthScopeSet userSet = await p.ReadScopeSetAsync( ctx, userInfo.UserId );
                    Assert.That( userSet.ToString(), Does.Contain( "[W]thing" ) );
                    Assert.That( userSet.ToString(), Does.Contain( "[W]other" ) );
                    Assert.That( userSet.ToString(), Does.Contain( "[W]nimp" ) );
                }
                await p.AuthScopeSetTable.SetScopesAsync( ctx, 1, original );
            }
        }

    }

}

