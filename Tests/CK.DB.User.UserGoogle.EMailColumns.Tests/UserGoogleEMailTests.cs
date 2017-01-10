using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.User.UserGoogle.EMailColumns.Tests
{
    [TestFixture]
    public class UserGoogleEMailTests
    {
        [Test]
        public void email_and_email_verified_are_managed()
        {
            var u = TestHelper.StObjMap.Default.Obtain<UserGoogleTable>();
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                string userName = "Google auth email - " + Guid.NewGuid().ToString();
                var googleAccountId = Guid.NewGuid().ToString( "N" );
                var idU = user.CreateUser( ctx, 1, userName );
                var info = u.CreateUserInfo();
                Assert.That( info, Is.InstanceOf<IUserGoogleInfoWithMail>() );
                IUserGoogleInfoWithMail infoM = (IUserGoogleInfoWithMail)info;
                infoM.EMail = "X@Y.Z";
                info.GoogleAccountId = googleAccountId;
                u.CreateOrUpdateGoogleUser( ctx, 1, idU, info );
                string rawSelect = $"select EMail collate Latin1_General_BIN2 +'|'+RefreshToken+'|'+cast(EMailVerified as varchar) from CK.tUserGoogle where UserId={idU}";
                u.Database.AssertScalarEquals( infoM.EMail+"||0", rawSelect );
                info.RefreshToken = "token";
                infoM.EMail = null;
                infoM.EMailVerified = true;
                u.CreateOrUpdateGoogleUser( ctx, 1, idU, info );
                u.Database.AssertScalarEquals( "X@Y.Z|token|1", rawSelect );
                infoM = (IUserGoogleInfoWithMail)u.FindKnownUserInfo( ctx, googleAccountId ).Info;
                Assert.That( infoM.EMailVerified, Is.True );
                Assert.That( infoM.EMail, Is.EqualTo( "X@Y.Z" ) );
            }
        }

    }
}
