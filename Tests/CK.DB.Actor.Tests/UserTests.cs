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
        public void Anonymous_can_not_create_a_user()
        {
            var map = TestHelper.StObjMap;
            var u = map.Default.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                Assert.Throws<SqlDetailedException>( () => u.CreateUser( ctx, 0, Guid.NewGuid().ToString() ) );
            }
        }

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

        [Test]
        public void UserName_is_not_set_by_default_if_another_user_exists_with_the_same_UserName()
        {
            var map = TestHelper.StObjMap;
            var u = map.Default.Obtain<UserTable>();

            string existingName = Guid.NewGuid().ToString();
            string userName = Guid.NewGuid().ToString();

            using( var ctx = new SqlStandardCallContext() )
            {
                int idExist = u.CreateUser( ctx, 1, existingName );
                int idUser = u.CreateUser( ctx, 1, userName );

                Assert.That( u.UserNameSet( ctx, 1, idUser, existingName )
                                && u.UserNameSet( ctx, 1, idExist, userName ), 
                             Is.False,
                             "No rename on clash." );

                Assert.That( u.UserNameSet( ctx, 1, idUser, userName ) 
                                && u.UserNameSet( ctx, 1, idExist, existingName ), 
                             "One can always rename to the current name." );

                u.DestroyUser( ctx, 1, idExist );
                u.DestroyUser( ctx, 1, idUser );

                u.Database.AssertEmptyReader( "select * from CK.tUser where UserName = @0 or UserName = @1", existingName, userName );
            }
        }

    }
}
