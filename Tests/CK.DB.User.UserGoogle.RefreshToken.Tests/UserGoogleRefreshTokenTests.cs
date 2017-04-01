using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CK.DB.User.UserGoogle.RefreshToken.Tests
{
    [TestFixture]
    public class UserGoogleRefreshTokenTests
    {
        [Test]
        public void RefreshToken_and_LastRefreshTokenTime_are_managed()
        {
            var google = TestHelper.StObjMap.Default.Obtain<UserGoogleTable>();
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                string userName = "Google RefreshToken - " + Guid.NewGuid().ToString();
                var googleAccountId = Guid.NewGuid().ToString( "N" );
                var idU = user.CreateUser( ctx, 1, userName );

                var info = google.CreateUserInfo<IUserGoogleInfo>();
                info.GoogleAccountId = googleAccountId;
                google.CreateOrUpdateGoogleUser( ctx, 1, idU, info );
                string rawSelect = $"select RefreshToken+'|'+cast(LastRefreshTokenTime as varchar) from CK.tUserGoogle where UserId={idU}";
                google.Database.AssertScalarEquals( "|0001-01-01 00:00:00.00", rawSelect );

                info.RefreshToken = "a refresh token";
                google.CreateOrUpdateGoogleUser( ctx, 1, idU, info );
                rawSelect = $"select RefreshToken from CK.tUserGoogle where UserId={idU}";
                google.Database.AssertScalarEquals( info.RefreshToken, rawSelect );

                info = (IUserGoogleInfo)google.FindKnownUserInfo( ctx, googleAccountId ).Info;
                Assert.That( info.LastRefreshTokenTime, Is.GreaterThan( DateTime.UtcNow.AddMonths(-1) ) );
                Assert.That( info.RefreshToken, Is.EqualTo("a refresh token") );

                var lastUpdate = info.LastRefreshTokenTime;
                Thread.Sleep(500);
                info.RefreshToken = null;
                google.CreateOrUpdateGoogleUser(ctx, 1, idU, info);
                info = (IUserGoogleInfo)google.FindKnownUserInfo( ctx, googleAccountId ).Info;
                Assert.That(info.LastRefreshTokenTime, Is.EqualTo(lastUpdate));
                Assert.That(info.RefreshToken, Is.EqualTo("a refresh token"));
            }
        }

    }
}
