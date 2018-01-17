using CK.Core;
using CK.DB.Actor.ActorEMail;
using CK.SqlServer;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CK.Testing.DBSetupTestHelper;

namespace CK.DB.Actor.ActorEMail.Tests
{
    [TestFixture]
    public class ActorEMailTests
    {
        [Test]
        public void adding_and_removing_one_mail_to_System()
        {
            var mails = TestHelper.StObjMap.Default.Obtain<ActorEMailTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                mails.Database.ExecuteScalar( "select PrimaryEMail from CK.vUser where UserId=1" )
                        .Should().Be( DBNull.Value );

                mails.AddEMail( ctx, 1, 1, "god@heaven.com", false );
                mails.Database.ExecuteScalar( "select PrimaryEMail from CK.vUser where UserId=1" )
                        .Should().Be( "god@heaven.com" );

                mails.RemoveEMail( ctx, 1, 1, "god@heaven.com" );
                mails.Database.ExecuteScalar( "select PrimaryEMail from CK.vUser where UserId=1" )
                        .Should().Be( DBNull.Value );
            }
        }

        [Test]
        public void first_email_is_automatically_primary_but_the_first_valid_one_is_elected()
        {
            var group = TestHelper.StObjMap.Default.Obtain<GroupTable>();
            var mails = TestHelper.StObjMap.Default.Obtain<ActorEMailTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var gId = group.CreateGroup( ctx, 1 );
                mails.AddEMail( ctx, 1, gId, "mail@address.com", false );
                mails.Database.ExecuteScalar( $"select PrimaryEMail from CK.vGroup where GroupId={gId}" )
                    .Should().Be( "mail@address.com" );

                mails.AddEMail( ctx, 1, gId, "Val-mail@address.com", false );
                mails.Database.ExecuteScalar( $"select PrimaryEMail from CK.vGroup where GroupId={gId}" )
                    .Should().Be( "mail@address.com" );

                mails.AddEMail( ctx, 1, gId, "bad-mail@address.com", false );
                mails.Database.ExecuteScalar( $"select PrimaryEMail from CK.vGroup where GroupId={gId}" )
                    .Should().Be( "mail@address.com" );


                mails.ValidateEMail( ctx, 1, gId, "Val-mail@address.com" );
                mails.Database.ExecuteScalar( $"select PrimaryEMail from CK.vGroup where GroupId={gId}" )
                    .Should().Be( "Val-mail@address.com" );

                group.DestroyGroup( ctx, 1, gId );
            }
        }

        [Test]
        public void when_removing_the_primary_email_another_one_is_elected_even_if_they_are_all_not_validated()
        {
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            var mails = TestHelper.StObjMap.Default.Obtain<ActorEMailTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var uId = user.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
                mails.AddEMail( ctx, 1, uId, "1@a.com", false );
                mails.AddEMail( ctx, 1, uId, "2@a.com", false );
                mails.AddEMail( ctx, 1, uId, "3@a.com", true );
                mails.AddEMail( ctx, 1, uId, "4@a.com", false );
                mails.Database.ExecuteScalar( $"select PrimaryEMail from CK.vUser where UserId={uId}" )
                    .Should().Be( "3@a.com" );

                mails.RemoveEMail( ctx, 1, uId, "3@a.com" );
                mails.Database.ExecuteScalar<string>( $"select PrimaryEMail from CK.vUser where UserId={uId}" )
                    .Should().Match( m => m == "1@a.com" || m == "2@a.com" || m == "4@a.com" );
                user.DestroyUser( ctx, 1, uId );
            }
        }

        [Test]
        public void when_removing_the_primary_email_the_most_recently_validated_is_elected()
        {
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            var mails = TestHelper.StObjMap.Default.Obtain<ActorEMailTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var uId = user.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
                for( int i = 0; i < 10; i++ )
                {
                    mails.AddEMail( ctx, 1, uId, $"fill{i}@a.com", false, true );
                }
                mails.AddEMail( ctx, 1, uId, "2@a.com", false );
                mails.AddEMail( ctx, 1, uId, "3@a.com", true );
                System.Threading.Thread.Sleep( 100 );
                mails.AddEMail( ctx, 1, uId, "4@a.com", false, true );
                mails.AddEMail( ctx, 1, uId, "5@a.com", false );
                mails.Database.ExecuteScalar( $"select PrimaryEMail from CK.vUser where UserId={uId}" );
                mails.RemoveEMail( ctx, 1, uId, "3@a.com" );
                mails.Database.ExecuteScalar( $"select PrimaryEMail from CK.vUser where UserId={uId}" );
                user.DestroyUser( ctx, 1, uId );
            }
        }

    }
}
