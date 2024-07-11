using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using CK.Testing;
using FluentAssertions;
using NUnit.Framework;
using System;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.User.UserGoogle.Profile.Tests
{
    [TestFixture]
    public class UserGoogleEMailTests
    {
        [Test]
        public void profile_properties_are_handled()
        {
            var u = SharedEngine.Map.StObjs.Obtain<UserGoogleTable>();
            var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var googleAccountId = Guid.NewGuid().ToString( "N" );
                string userName = "Google user " + googleAccountId;
                var idU = user.CreateUser( ctx, 1, userName );
                var info = u.CreateUserInfo<IUserGoogleInfo>();
                info.GoogleAccountId = googleAccountId;
                info.FirstName = "Albert";
                info.LastName = "Einstein";
                u.CreateOrUpdateGoogleUser( ctx, 1, idU, info );
                string rawSelect = $"select FirstName collate Latin1_General_BIN2+'|'+LastName  collate Latin1_General_BIN2+'|'+UserName  collate Latin1_General_BIN2+'|'+PictureUrl  collate Latin1_General_BIN2 from CK.tUserGoogle where UserId={idU}";
                u.Database.ExecuteScalar( rawSelect ).Should().Be( "Albert|Einstein||" );
                info.FirstName = null;
                info.LastName = null;
                info.UserName = "Bebert";
                info.PictureUrl = "url";
                u.CreateOrUpdateGoogleUser( ctx, 1, idU, info );
                u.Database.ExecuteScalar( rawSelect ).Should().Be( "Albert|Einstein|Bebert|url" );

                info = (Profile.IUserGoogleInfo)u.FindKnownUserInfo( ctx, googleAccountId ).Info;
                info.FirstName.Should().Be( "Albert" );
                info.LastName.Should().Be( "Einstein" );
                info.UserName.Should().Be( "Bebert" );
                info.PictureUrl.Should().Be( "url" );
            }
        }

    }
}
