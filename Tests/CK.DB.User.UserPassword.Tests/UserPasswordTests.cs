using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using NUnit.Framework;
using System.Linq;

namespace CK.DB.User.UserPassword.Tests
{
    [TestFixture]
    public class UserPasswordTests
    {

        [Test]
        public void create_password_and_check_Verify_method()
        {
            var u = TestHelper.StObjMap.Default.Obtain<UserPasswordTable>();
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var userName = Guid.NewGuid().ToString();
                int userId = user.CreateUser( ctx, 1, userName );
                var pwd = "pwddetestcrrr";
                var pwd2 = "pwddetestcrdfezfrefzzfrr";

                u.CreatePasswordUser( ctx, 1, userId, pwd );
                Assert.That( u.Verify( ctx, userId, pwd ) );
                Assert.That( u.Verify( ctx, userId, pwd2 ), Is.False );

                u.SetPassword( ctx, 1, userId, pwd2 );
                Assert.That( u.Verify( ctx, userId, pwd2 ) );
                Assert.That( u.Verify( ctx, userId, pwd ), Is.False );

            }
        }

        [Test]
        public void create_a_password_for_an_anonymous_user_is_an_error()
        {
            var u = TestHelper.StObjMap.Default.Obtain<UserPasswordTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                Assert.Throws<SqlDetailedException>( () => u.CreatePasswordUser( ctx, 1, 0, "x" ) );
                Assert.Throws<SqlDetailedException>( () => u.CreatePasswordUser( ctx, 0, 1, "toto" ) );
            }
        }

        [Test]
        public void destroying_a_user_destroys_its_PasswordUser_facet()
        {
            var u = TestHelper.StObjMap.Default.Obtain<UserPasswordTable>();
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int userId = user.CreateUser( ctx, 1, Guid.NewGuid().ToString() );
                u.CreatePasswordUser( ctx, 1, userId, "pwd" );
                user.DestroyUser( ctx, 1, userId );
            }
        }

        [TestCase( "p" )]
        [TestCase( "deefzrfgebhntjuykilompo^ùp$*pù^mlkjhgf250258p" )]
        public void changing_iteration_count_updates_automatically_the_hash( string pwd )
        {
            var u = TestHelper.StObjMap.Default.Obtain<UserPasswordTable>();
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                UserPasswordTable.HashIterationCount = 1000;
                var userName = Guid.NewGuid().ToString();
                int userId = user.CreateUser( ctx, 1, userName );
                u.CreatePasswordUser( ctx, 1, userId, pwd );
                var hash1 = u.Database.ExecuteScalar<byte[]>( $"select PwdHash from CK.tUserPassword where UserId={userId}" );

                UserPasswordTable.HashIterationCount = 2000;
                Assert.That( u.Verify( ctx, userId, pwd ) );
                var hash2 = u.Database.ExecuteScalar<byte[]>( $"select PwdHash from CK.tUserPassword where UserId={userId}" );

                Assert.That( hash1.SequenceEqual( hash2 ), Is.False, "Hash has been updated." );

                UserPasswordTable.HashIterationCount = UserPasswordTable.DefaultHashIterationCount;
                Assert.That( u.Verify( ctx, userId, pwd ) );
                var hash3 = u.Database.ExecuteScalar<byte[]>( $"select PwdHash from CK.tUserPassword where UserId={userId}" );

                Assert.That( hash1.SequenceEqual( hash3 ), Is.False, "Hash has been updated." );
                Assert.That( hash2.SequenceEqual( hash3 ), Is.False, "Hash has been updated." );

            }
        }

        class MigrationSupport : IUserPasswordMigrator
        {
            readonly int _userIdToMigrate;
            readonly string _pwd;

            public bool MigrationDoneCalled;

            public MigrationSupport( int userIdToMigrate, string pwd )
            {
                _userIdToMigrate = userIdToMigrate;
                _pwd = pwd;
            }

            public void MigrationDone( ISqlCallContext ctx, int userId ) => MigrationDoneCalled = true;

            public bool VerifyPassword( ISqlCallContext ctx, int userId, string password )
            {
                return userId == _userIdToMigrate && _pwd == password; 
            }
        }

        [Test]
        public void password_migration_is_supported_by_user_id_and_user_name()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            var u = TestHelper.StObjMap.Default.Obtain<UserPasswordTable>();
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                // By identifier
                {
                    string userName = Guid.NewGuid().ToString();
                    var idU = user.CreateUser( ctx, 1, userName );
                    p.PasswordMigrator = new MigrationSupport( idU, "toto" );
                    Assert.That( u.Verify( ctx, idU, "failed" ), Is.False );
                    p.Database.AssertEmptyReader( $"select 1 from CK.tUserPassword where UserId={idU}" );
                    Assert.That( u.Verify( ctx, idU, "toto" ) );
                    p.Database.AssertScalarEquals( 1, $"select 1 from CK.tUserPassword where UserId={idU}" );
                    Assert.That( u.Verify( ctx, idU, "toto" ) );
                }
                // By user name
                {
                    string userName = Guid.NewGuid().ToString();
                    var idU = user.CreateUser( ctx, 1, userName );
                    p.PasswordMigrator = new MigrationSupport( idU, "toto" );
                    Assert.That( u.Verify( ctx, userName, "failed" ), Is.False );
                    p.Database.AssertEmptyReader( $"select 1 from CK.tUserPassword where UserId={idU}" );
                    Assert.That( u.Verify( ctx, userName, "toto" ) );
                    p.Database.AssertScalarEquals( 1, $"select 1 from CK.tUserPassword where UserId={idU}" );
                    Assert.That( u.Verify( ctx, userName, "toto" ) );
                }
            }
        }
        [Test]
        public async Task password_migration_is_supported_by_user_id_and_user_name_async()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            var u = TestHelper.StObjMap.Default.Obtain<UserPasswordTable>();
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                // By identifier
                {
                    string userName = Guid.NewGuid().ToString();
                    var idU = await user.CreateUserAsync( ctx, 1, userName );
                    p.PasswordMigrator = new MigrationSupport( idU, "toto" );
                    Assert.That( await u.VerifyAsync( ctx, idU, "failed" ), Is.False );
                    p.Database.AssertEmptyReader( $"select 1 from CK.tUserPassword where UserId={idU}" );
                    Assert.That( await u.VerifyAsync( ctx, idU, "toto" ) );
                    p.Database.AssertScalarEquals( 1, $"select 1 from CK.tUserPassword where UserId={idU}" );
                    Assert.That( await u.VerifyAsync( ctx, idU, "toto" ) );
                }
                // By user name
                {
                    string userName = Guid.NewGuid().ToString();
                    var idU = await user.CreateUserAsync( ctx, 1, userName );
                    p.PasswordMigrator = new MigrationSupport( idU, "toto" );
                    Assert.That( await u.VerifyAsync( ctx, userName, "failed" ), Is.False );
                    p.Database.AssertEmptyReader( $"select 1 from CK.tUserPassword where UserId={idU}" );
                    Assert.That( await u.VerifyAsync( ctx, userName, "toto" ) );
                    p.Database.AssertScalarEquals( 1, $"select 1 from CK.tUserPassword where UserId={idU}" );
                    Assert.That( await u.VerifyAsync( ctx, userName, "toto" ) );
                }
            }
        }
    }
}
