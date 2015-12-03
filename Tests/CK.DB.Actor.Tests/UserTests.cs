using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using CK.SqlServer;
using CK.Core;
using System.Data.SqlClient;

namespace CK.DB.Actor.Tests
{
    [TestFixture]
    public class UserTests
    {
        [Test]
        public void user_can_not_be_created_with_an_already_existing_UserName()
        {
            var map = TestHelper.StObjMap;
            var u = map.Default.Obtain<UserTable>();

            string testName = "user_can_not_be_created_with_an_already_existing_UserName" + Guid.NewGuid().ToString();

            using( var ctx = new SqlStandardCallContext() )
            {
                int id = u.CreateUser( ctx, 1, testName );
                Assert.That( id, Is.GreaterThan( 1 ) );

                int idRejected = u.CreateUser( ctx, 1, testName );
                Assert.That( idRejected, Is.EqualTo( -1 ) );

                u.DestroyUser( ctx, 1, id );

                u.Database.AssertEmptyReader( "select * from CK.tUser where UserName = @0", testName );
            }
        }

    }
}
