using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using NUnit.Framework;

namespace CK.DB.User.UserPassword.Tests
{
    [TestFixture]
    public class UserPasswordTests
    {

        [Test]
        public async Task create_passwordHash_to_a_user()
        {
            var u = TestHelper.StObjMap.Default.Obtain<UserPasswordTable>();
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            using (var ctx = new SqlStandardCallContext())
            {
                var userName = Guid.NewGuid().ToString();
                int uid = await user.CreateUserAsync(ctx, 1, userName);
                var pwd = "pwddetestcrrr";
                var pwd2 = "pwddetestcrdfezfrefzzfrr";

                await u.CreateAsync(ctx, 1, uid, pwd);
                Assert.That(await u.Verify(ctx, uid, pwd));
                Assert.That(await u.Verify(ctx, uid, pwd2), Is.False);

                await u.SetPasswordAsync(ctx, 1, uid, pwd2);
                Assert.That(await u.Verify(ctx, uid, pwd2));
                Assert.That(await u.Verify(ctx, uid, pwd), Is.False);

            }
        }

        [Test]
        public async Task create_a_password_for_an_anonymous_user_is_an_error()
        {
            var u = TestHelper.StObjMap.Default.Obtain<UserPasswordTable>();
            using (var ctx = new SqlStandardCallContext())
            {
                Assert.Throws<SqlException>( async () => await u.CreateAsync(ctx, 1, 0, "x") );
                Assert.Throws<SqlException>( async () => await u.CreateAsync(ctx, 0, 1, "toto") );
            }
        }

        [TestCase("p")]
        [TestCase("deefzrfgebhntjuykilompo^ùp$*pù^mlkjhgf250258p")]
        public async Task Changing_iteration_count_updates_automatically_the_hash( string pwd)
        {
            var u = TestHelper.StObjMap.Default.Obtain<UserPasswordTable>();
            var user = TestHelper.StObjMap.Default.Obtain<UserTable>();
            using (var ctx = new SqlStandardCallContext())
            {
                UserPasswordTable.HashIterationCount = 1;
                var userName = Guid.NewGuid().ToString();
                int uid = await user.CreateUserAsync(ctx, 1, userName);              
                await u.CreateAsync(ctx, 1, uid, pwd);

                 UserPasswordTable.HashIterationCount = 2;
                Assert.That(await u.Verify(ctx, uid, pwd)); 
            }
        }
    }
}
