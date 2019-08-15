using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading;
using static CK.Testing.DBSetupTestHelper;

namespace CK.DB.User.UserGoogle.RefreshToken.Tests
{
    [TestFixture]
    public class UserGoogleRefreshTokenTests
    {
        [Test]
        public void RefreshToken_and_LastRefreshTokenTime_are_managed()
        {
            var google = TestHelper.StObjMap.StObjs.Obtain<UserGoogleTable>();
            var user = TestHelper.StObjMap.StObjs.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                string userName = "Google RefreshToken - " + Guid.NewGuid().ToString();
                var googleAccountId = Guid.NewGuid().ToString( "N" );
                var idU = user.CreateUser( ctx, 1, userName );

                var info = google.CreateUserInfo<IUserGoogleInfo>();
                info.GoogleAccountId = googleAccountId;
                google.CreateOrUpdateGoogleUser( ctx, 1, idU, info );
                string rawSelect = $"select RefreshToken+'|'+cast(LastRefreshTokenTime as varchar) from CK.tUserGoogle where UserId={idU}";
                google.Database.ExecuteScalar( rawSelect )
                    .Should().Be( "|0001-01-01 00:00:00.00" );

                info.RefreshToken = "a refresh token";
                google.CreateOrUpdateGoogleUser( ctx, 1, idU, info );
                rawSelect = $"select RefreshToken from CK.tUserGoogle where UserId={idU}";
                google.Database.ExecuteScalar( rawSelect )
                    .Should().Be( info.RefreshToken );

                info = (IUserGoogleInfo)google.FindKnownUserInfo( ctx, googleAccountId ).Info;
                info.LastRefreshTokenTime.Should().BeAfter( DateTime.UtcNow.AddMonths( -1 ) );
                info.RefreshToken.Should().Be( "a refresh token" );

                var lastUpdate = info.LastRefreshTokenTime;
                Thread.Sleep( 500 );
                info.RefreshToken = null;
                google.CreateOrUpdateGoogleUser( ctx, 1, idU, info );
                info = (IUserGoogleInfo)google.FindKnownUserInfo( ctx, googleAccountId ).Info;
                info.LastRefreshTokenTime.Should().Be( lastUpdate );
                info.RefreshToken.Should().Be( "a refresh token" );
            }
        }

    }
}
