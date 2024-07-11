using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using CK.Testing;
using FluentAssertions;
using NUnit.Framework;
using System;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.User.UserGoogle.EMailColumns.Tests
{
    [TestFixture]
    public class UserGoogleEMailTests
    {
        [Test]
        public void email_and_email_verified_are_managed()
        {
            var u = SharedEngine.Map.StObjs.Obtain<UserGoogleTable>();
            var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                string userName = "Google auth email - " + Guid.NewGuid().ToString();
                var googleAccountId = Guid.NewGuid().ToString( "N" );
                var idU = user.CreateUser( ctx, 1, userName );
                var info = u.CreateUserInfo<IUserGoogleInfo>();
                info.EMail = "X@Y.Z";
                info.GoogleAccountId = googleAccountId;
                u.CreateOrUpdateGoogleUser( ctx, 1, idU, info );
                string rawSelect = $"select EMail collate Latin1_General_BIN2+'|'+cast(EMailVerified as varchar) from CK.tUserGoogle where UserId={idU}";
                u.Database.ExecuteScalar( rawSelect )
                    .Should().Be( info.EMail + "|0" );
                info.EMail = null;
                info.EMailVerified = true;
                u.CreateOrUpdateGoogleUser( ctx, 1, idU, info );
                u.Database.ExecuteScalar( rawSelect )
                    .Should().Be( "X@Y.Z|1" );
                info = (EMailColumns.IUserGoogleInfo)u.FindKnownUserInfo( ctx, googleAccountId ).Info;
                info.EMailVerified.Should().BeTrue();
                info.EMail.Should().Be( "X@Y.Z" );
            }
        }

    }
}
