using CK.Core;
using CK.SqlServer;
using FluentAssertions;
using NUnit.Framework;
using System;
using static CK.Testing.DBSetupTestHelper;

namespace CK.DB.Res.ResName.Tests
{
    [TestFixture]
    public class ResNameTests
    {
        [Test]
        public void resource_0_and_1_are_empty_and_System()
        {
            var r = TestHelper.StObjMap.StObjs.Obtain<ResNameTable>();
            r.Database.ExecuteScalar( "select ResName from CK.vRes where ResId = 0" )
                .Should().Be( "" );
            r.Database.ExecuteScalar( "select ResName from CK.vRes where ResId = 1" )
                .Should().Be( "System" );
        }


        [Test]
        public void resource_0_and_1_can_not_be_destroyed()
        {
            var p = TestHelper.StObjMap.StObjs.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                p.Invoking( sut => sut.ResTable.Destroy( ctx, 0 ) ).Should().Throw<SqlDetailedException>();
                p.Invoking( sut => sut.ResTable.Destroy( ctx, 1 ) ).Should().Throw<SqlDetailedException>();
            }
        }

        [Test]
        public void CreateResName_raises_an_exception_if_the_resource_is_already_associated_to_a_name_or_the_name_already_exists()
        {
            var p = TestHelper.StObjMap.StObjs.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int resId = p.ResTable.Create( ctx );
                string resName = Guid.NewGuid().ToString();
                string resName2 = Guid.NewGuid().ToString();
                p.ResNameTable.CreateResName( ctx, resId, resName );
                // Creates where a name already exists.
                p.Invoking( sut => sut.ResNameTable.CreateResName( ctx, resId, resName2 ) ).Should().Throw<SqlDetailedException>();
                // Creates with an already existing name.
                int resId2 = p.ResTable.Create( ctx );
                p.Invoking( sut => sut.ResNameTable.CreateResName( ctx, resId2, resName ) ).Should().Throw<SqlDetailedException>();
                p.ResTable.Destroy( ctx, resId );
                p.ResTable.Destroy( ctx, resId2 );
            }
        }

        [Test]
        public void renaming_a_resource_can_be_done_WithChildren_or_only_for_the_resource_itself()
        {
            var p = TestHelper.StObjMap.StObjs.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                p.ResNameTable.DestroyByResNamePrefix( ctx, "Test", resNameOnly: false );

                int n1 = p.ResNameTable.CreateWithResName( ctx, "Test.Root" );
                int n2 = p.ResNameTable.CreateWithResName( ctx, "Test.Root.1" );
                int n3 = p.ResNameTable.CreateWithResName( ctx, "Test.Root.1.1" );

                p.ResNameTable.Rename( ctx, n1, "Test.-Root-" );
                p.Database.ExecuteReader( "select * from CK.tResName where ResName like 'Test.Root%'" )
                    .Rows.Should().BeEmpty();
                p.Database.ExecuteScalar( "select ResId from CK.tResName where ResName='Test.-Root-'" )
                    .Should().Be( n1 );
                p.Database.ExecuteScalar( "select ResId from CK.tResName where ResName='Test.-Root-.1'" )
                    .Should().Be( n2 );
                p.Database.ExecuteScalar( "select ResId from CK.tResName where ResName='Test.-Root-.1.1'" )
                .Should().Be( n3 );

                p.ResNameTable.Rename( ctx, n1, "Test.MovedTheRootOnly", false );
                p.Database.ExecuteScalar( "select ResId from CK.tResName where ResName='Test.MovedTheRootOnly'" )
                     .Should().Be( n1 );
                p.Database.ExecuteScalar( "select ResId from CK.tResName where ResName='Test.-Root-.1'" )
                     .Should().Be( n2 );
                p.Database.ExecuteScalar( "select ResId from CK.tResName where ResName='Test.-Root-.1.1'" )
                    .Should().Be( n3 );
            }
        }

        [Test]
        public void using_DestroyByPrefix_enables_destruction_without_an_existing_parent()
        {
            var p = TestHelper.StObjMap.StObjs.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var nameRoot = Guid.NewGuid().ToString();

                int n1 = p.ResNameTable.CreateWithResName( ctx, nameRoot + ".Test.Root" );
                int n2 = p.ResNameTable.CreateWithResName( ctx, nameRoot + ".Test.Root.1" );
                int n3 = p.ResNameTable.CreateWithResName( ctx, nameRoot + ".Test.Root.1.1" );
                p.Database.ExecuteScalar( "select count(*) from CK.tResName where ResName like @0+'%'", nameRoot )
                    .Should().Be( 3 );

                p.ResNameTable.DestroyByResNamePrefix( ctx, nameRoot, resNameOnly: false );
                p.Database.ExecuteReader( "select * from CK.tResName where ResName like @0 + '%'", nameRoot )
                    .Rows.Should().BeEmpty();
            }
        }
    }
}
