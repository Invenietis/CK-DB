using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using CK.Testing;
using Shouldly;
using NUnit.Framework;
using System;
using System.Threading;

namespace CK.DB.User.UserGoogle.RefreshToken.Tests;

[TestFixture]
public class UserGoogleRefreshTokenTests
{
    [Test]
    public void RefreshToken_and_LastRefreshTokenTime_are_managed()
    {
        var google = SharedEngine.Map.StObjs.Obtain<UserGoogleTable>();
        var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
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
                .ShouldBe( "|0001-01-01 00:00:00.00" );

            info.RefreshToken = "a refresh token";
            google.CreateOrUpdateGoogleUser( ctx, 1, idU, info );
            rawSelect = $"select RefreshToken from CK.tUserGoogle where UserId={idU}";
            google.Database.ExecuteScalar( rawSelect )
                .ShouldBe( info.RefreshToken );

            info = (IUserGoogleInfo)google.FindKnownUserInfo( ctx, googleAccountId ).Info;
            info.LastRefreshTokenTime.ShouldBeGreaterThan( DateTime.UtcNow.AddMonths( -1 ) );
            info.RefreshToken.ShouldBe( "a refresh token" );

            var lastUpdate = info.LastRefreshTokenTime;
            Thread.Sleep( 500 );
            info.RefreshToken = null;
            google.CreateOrUpdateGoogleUser( ctx, 1, idU, info );
            info = (IUserGoogleInfo)google.FindKnownUserInfo( ctx, googleAccountId ).Info;
            info.LastRefreshTokenTime.ShouldBe( lastUpdate );
            info.RefreshToken.ShouldBe( "a refresh token" );
        }
    }

}
