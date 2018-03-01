using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using CK.DB.Auth;
using System.Collections.Generic;
using FluentAssertions;
using static CK.Testing.DBSetupTestHelper;

namespace CK.DB.User.UserPassword.Tests
{
    [TestFixture]
    public class UserPasswordTests
    {

        [Test]
        public void standard_generic_tests_for_Basic_provider()
        {
            var auth = TestHelper.StObjMap.Default.Obtain<Auth.Package>();
            CK.DB.Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProvider(
                auth,
                "Basic",
                payloadForCreateOrUpdate: ( userId, userName ) => "pwd",
                payloadForLogin: ( userId, userName ) => Tuple.Create( userId, "pwd" ),
                payloadForLoginFail: ( userId, userName ) => Tuple.Create( userId, "PWD" )
                );
        }

        [Test]
        public void Generic_to_Basic_provider_with_userId_as_double_or_as_string()
        {
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            var auth = TestHelper.StObjMap.Default.Obtain<Auth.Package>();
            var basic = auth.FindProvider( "Basic" );
            using( var ctx = new SqlStandardCallContext() )
            {
                var userName = Guid.NewGuid().ToString();
                var userId = user.CreateUser( ctx, 1, userName );
                basic.CreateOrUpdateUser( ctx, 1, userId, "pass" ).OperationResult.Should().Be( UCResult.Created );
                var payload = new Dictionary<string, object>();
                payload["password"] = "pass";

                payload["userId"] = (double)userId;
                basic.LoginUser( ctx, payload ).IsSuccess.Should().BeTrue();

                payload["userId"] = userId.ToString();
                basic.LoginUser( ctx, payload ).IsSuccess.Should().BeTrue();
                user.DestroyUser( ctx, 1, userId );
            }
        }

        [Test]
        public async Task standard_generic_tests_for_Basic_provider_Async()
        {
            var auth = TestHelper.StObjMap.Default.Obtain<Auth.Package>();
            await Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProviderAsync(
                auth,
                "Basic",
                payloadForCreateOrUpdate: ( userId, userName ) => "pwd",
                payloadForLogin: ( userId, userName ) => Tuple.Create( userId, "pwd" ),
                payloadForLoginFail: ( userId, userName ) => Tuple.Create( userId, "PWD" )
                );
        }


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

                u.CreateOrUpdatePasswordUser( ctx, 1, userId, pwd ).OperationResult.Should().Be( UCResult.Created );
                u.LoginUser( ctx, userId, pwd ).UserId.Should().Be( userId );
                u.LoginUser( ctx, userId, pwd2 ).UserId.Should().Be( 0 );

                u.SetPassword( ctx, 1, userId, pwd2 );
                u.LoginUser( ctx, userId, pwd2 ).UserId.Should().Be( userId );
                u.LoginUser( ctx, userId, pwd ).UserId.Should().Be( 0 );

            }
        }

        [Test]
        public void create_a_password_for_an_anonymous_user_is_an_error()
        {
            var u = TestHelper.StObjMap.Default.Obtain<UserPasswordTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                u.Invoking( sut => sut.CreateOrUpdatePasswordUser( ctx, 1, 0, "x" ) ).ShouldThrow<SqlDetailedException>();
                u.Invoking( sut => sut.CreateOrUpdatePasswordUser( ctx, 0, 1, "toto" ) ).ShouldThrow<SqlDetailedException>();
                u.Invoking( sut => sut.CreateOrUpdatePasswordUser( ctx, 1, 0, "x", UCLMode.UpdateOnly ) ).ShouldThrow<SqlDetailedException>();
                u.Invoking( sut => sut.CreateOrUpdatePasswordUser( ctx, 0, 1, "toto", UCLMode.UpdateOnly ) ).ShouldThrow<SqlDetailedException>();
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
                u.CreateOrUpdatePasswordUser( ctx, 1, userId, "pwd" );
                user.DestroyUser( ctx, 1, userId );
                u.Database.ExecuteReader( "select * from CK.tUserPassword where UserId = @0", userId )
                    .Rows.Should().BeEmpty();
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
                u.CreateOrUpdatePasswordUser( ctx, 1, userId, pwd );
                var hash1 = u.Database.ExecuteScalar<byte[]>( $"select PwdHash from CK.tUserPassword where UserId={userId}" );

                UserPasswordTable.HashIterationCount = 2000;
                u.LoginUser( ctx, userId, pwd ).UserId.Should().Be( userId );
                var hash2 = u.Database.ExecuteScalar<byte[]>( $"select PwdHash from CK.tUserPassword where UserId={userId}" );

                hash1.SequenceEqual( hash2 ).Should().BeFalse( "Hash has been updated." );

                UserPasswordTable.HashIterationCount = UserPasswordTable.DefaultHashIterationCount;
                u.LoginUser( ctx, userId, pwd ).UserId.Should().Be( userId );
                var hash3 = u.Database.ExecuteScalar<byte[]>( $"select PwdHash from CK.tUserPassword where UserId={userId}" );

                hash1.SequenceEqual( hash3 ).Should().BeFalse( "Hash has been updated." );
                hash2.SequenceEqual( hash3 ).Should().BeFalse( "Hash has been updated." );

            }
        }

        [Test]
        public void UserPassword_implements_IBasicAuthenticationProvider()
        {
            var basic = TestHelper.StObjMap.Default.Obtain<IBasicAuthenticationProvider>();
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                string name = Guid.NewGuid().ToString();
                int userId = user.CreateUser( ctx, 1, name );
                string pwd = "lklkl";
                var result = basic.CreateOrUpdatePasswordUser( ctx, 1, userId, pwd, UCLMode.CreateOnly );
                result.OperationResult.Should().Be( UCResult.Created );
                result = basic.CreateOrUpdatePasswordUser( ctx, 1, userId, pwd + "no", UCLMode.CreateOnly );
                result.OperationResult.Should().Be( UCResult.None );
                basic.LoginUser( ctx, userId, pwd ).UserId.Should().Be( userId );
                basic.LoginUser( ctx, userId, pwd + "no" ).UserId.Should().Be( 0 );
                basic.LoginUser( ctx, name, pwd ).UserId.Should().Be( userId );
                basic.LoginUser( ctx, name, pwd + "no" ).UserId.Should().Be( 0 );
                basic.SetPassword( ctx, 1, userId, (pwd = pwd + "BIS") );
                basic.LoginUser( ctx, userId, pwd ).UserId.Should().Be( userId );
                basic.LoginUser( ctx, userId, pwd + "no" ).UserId.Should().Be( 0 );
                basic.LoginUser( ctx, name, pwd ).UserId.Should().Be( userId );
                basic.LoginUser( ctx, name, pwd + "no" ).UserId.Should().Be( 0 );
                basic.DestroyPasswordUser( ctx, 1, userId );
                user.Database.ExecuteReader( "select * from CK.tUserPassword where UserId = @0", userId )
                    .Rows.Should().BeEmpty();
                user.DestroyUser( ctx, 1, userId );
            }
        }

        [Test]
        public async Task UserPassword_implements_IBasicAuthenticationProvider_async()
        {
            var basic = TestHelper.StObjMap.Default.Obtain<IBasicAuthenticationProvider>();
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                string name = Guid.NewGuid().ToString();
                int userId = await user.CreateUserAsync( ctx, 1, name );
                string pwd = "lklkl";
                var result = await basic.CreateOrUpdatePasswordUserAsync( ctx, 1, userId, pwd, UCLMode.CreateOnly );
                result.OperationResult.Should().Be( UCResult.Created );
                result = await basic.CreateOrUpdatePasswordUserAsync( ctx, 1, userId, pwd + "no", UCLMode.CreateOnly );
                result.OperationResult.Should().Be( UCResult.None );
                (await basic.LoginUserAsync( ctx, userId, pwd )).UserId.Should().Be( userId );
                (await basic.LoginUserAsync( ctx, userId, pwd + "no" )).UserId.Should().Be( 0 );
                (await basic.LoginUserAsync( ctx, name, pwd )).UserId.Should().Be( userId );
                (await basic.LoginUserAsync( ctx, name, pwd + "no" )).UserId.Should().Be( 0 );
                await basic.SetPasswordAsync( ctx, 1, userId, (pwd = pwd + "BIS") );
                (await basic.LoginUserAsync( ctx, userId, pwd )).UserId.Should().Be( userId );
                (await basic.LoginUserAsync( ctx, userId, pwd + "no" )).UserId.Should().Be( 0 );
                (await basic.LoginUserAsync( ctx, name, pwd )).UserId.Should().Be( userId );
                (await basic.LoginUserAsync( ctx, name, pwd + "no" )).UserId.Should().Be( 0 );
                await basic.DestroyPasswordUserAsync( ctx, 1, userId );
                user.Database.ExecuteReader( "select * from CK.tUserPassword where UserId = @0", userId )
                    .Rows.Should().BeEmpty();
                await user.DestroyUserAsync( ctx, 1, userId );
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
                    u.LoginUser( ctx, idU, "failed" ).UserId.Should().Be( 0 );
                    p.Database.ExecuteReader( $"select 1 from CK.tUserPassword where UserId={idU}" )
                        .Rows.Should().BeEmpty();
                    u.LoginUser( ctx, idU, "toto" ).UserId.Should().Be( idU );
                    p.Database.ExecuteScalar( $"select 1 from CK.tUserPassword where UserId={idU}" )
                        .Should().Be( 1 );
                    u.LoginUser( ctx, idU, "toto" ).UserId.Should().Be( idU );
                }
                // By user name
                {
                    string userName = Guid.NewGuid().ToString();
                    var idU = user.CreateUser( ctx, 1, userName );
                    p.PasswordMigrator = new MigrationSupport( idU, "toto" );
                    u.LoginUser( ctx, userName, "failed" ).UserId.Should().Be( 0 );
                    p.Database.ExecuteReader( $"select 1 from CK.tUserPassword where UserId={idU}" )
                        .Rows.Should().BeEmpty();
                    u.LoginUser( ctx, userName, "toto" ).UserId.Should().Be( idU );
                    p.Database.ExecuteScalar( $"select 1 from CK.tUserPassword where UserId={idU}" )
                        .Should().Be( 1 );
                    u.LoginUser( ctx, userName, "toto" ).UserId.Should().Be( idU );
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
                    (await u.LoginUserAsync( ctx, idU, "failed" )).UserId.Should().Be( 0 );
                    p.Database.ExecuteReader( $"select 1 from CK.tUserPassword where UserId={idU}" )
                        .Rows.Should().BeEmpty();
                    (await u.LoginUserAsync( ctx, idU, "toto" )).UserId.Should().Be( idU );
                    p.Database.ExecuteScalar( $"select 1 from CK.tUserPassword where UserId={idU}" )
                        .Should().Be( 1 );
                    (await u.LoginUserAsync( ctx, idU, "toto" )).UserId.Should().Be( idU );
                }
                // By user name
                {
                    string userName = Guid.NewGuid().ToString();
                    var idU = await user.CreateUserAsync( ctx, 1, userName );
                    p.PasswordMigrator = new MigrationSupport( idU, "toto" );
                    (await u.LoginUserAsync( ctx, userName, "failed" )).UserId.Should().Be( 0 );
                    p.Database.ExecuteReader( $"select 1 from CK.tUserPassword where UserId={idU}" )
                        .Rows.Should().BeEmpty();
                    (await u.LoginUserAsync( ctx, userName, "toto" )).UserId.Should().Be( idU );
                    p.Database.ExecuteScalar( $"select 1 from CK.tUserPassword where UserId={idU}" )
                        .Should().Be( 1 );
                    (await u.LoginUserAsync( ctx, userName, "toto" )).UserId.Should().Be( idU );
                }
            }
        }

        [Test]
        public void onLogin_extension_point_is_called()
        {
            var u = TestHelper.StObjMap.Default.Obtain<UserPasswordTable>();
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                // By name
                {
                    string userName = Guid.NewGuid().ToString();
                    var idU = user.CreateUser( ctx, 1, userName );
                    var baseTime = u.Database.ExecuteScalar<DateTime>( "select sysutcdatetime();" );
                    u.CreateOrUpdatePasswordUser( ctx, 1, idU, "password", UCLMode.CreateOrUpdate | UCLMode.WithActualLogin );
                    var firstTime = u.Database.ExecuteScalar<DateTime>( $"select LastLoginTime from CK.tUserPassword where UserId={idU}" );
                    firstTime.Should().BeCloseTo( baseTime, 1000 );
                    Thread.Sleep( 100 );
                    u.LoginUser( ctx, userName, "failed login", actualLogin: true ).UserId.Should().Be( 0 );
                    var firstTimeNo = u.Database.ExecuteScalar<DateTime>( $"select LastLoginTime from CK.tUserPassword where UserId={idU}" );
                    firstTimeNo.Should().Be( firstTime );
                    u.LoginUser( ctx, userName, "password", actualLogin: true ).UserId.Should().Be( idU );
                    var firstTimeYes = u.Database.ExecuteScalar<DateTime>( $"select LastLoginTime from CK.tUserPassword where UserId={idU}" );
                    firstTimeYes.Should().BeAfter( firstTimeNo );
                }
            }
        }

        [Test]
        public void Basic_AuthProvider_is_registered()
        {
            Auth.Tests.AuthTests.CheckProviderRegistration( "Basic" );
        }

        [Test]
        public void vUserAuthProvider_reflects_the_user_basic_authentication()
        {
            var u = TestHelper.StObjMap.Default.Obtain<UserPasswordTable>();
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                string userName = "Basic auth - " + Guid.NewGuid().ToString();
                var idU = user.CreateUser( ctx, 1, userName );
                u.Database.ExecuteReader( $"select * from CK.vUserAuthProvider where UserId={idU} and Scheme='Basic'" )
                    .Rows.Should().BeEmpty();
                u.CreateOrUpdatePasswordUser( ctx, 1, idU, "password" );
                u.Database.ExecuteScalar( $"select count(*) from CK.vUserAuthProvider where UserId={idU} and Scheme='Basic'" )
                    .Should().Be( 1 );
                u.DestroyPasswordUser( ctx, 1, idU );
                u.Database.ExecuteReader( $"select * from CK.vUserAuthProvider where UserId={idU} and Scheme='Basic'" )
                    .Rows.Should().BeEmpty();
                // To let the use in the database with a basic authentication.
                u.CreateOrUpdatePasswordUser( ctx, 1, idU, "password" );
            }
        }

    }
}
