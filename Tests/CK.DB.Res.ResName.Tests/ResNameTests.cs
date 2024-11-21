using CK.Core;
using CK.SqlServer;
using CK.Testing;
using FluentAssertions;
using NUnit.Framework;
using System;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.Res.ResName.Tests;

[TestFixture]
public class ResNameTests
{
    [Test]
    public void resource_0_and_1_are_empty_and_System()
    {
        var r = SharedEngine.Map.StObjs.Obtain<ResNameTable>();
        r.Database.ExecuteScalar( "select ResName from CK.vRes where ResId = 0" )
            .Should().Be( "" );
        r.Database.ExecuteScalar( "select ResName from CK.vRes where ResId = 1" )
            .Should().Be( "System" );
    }


    [Test]
    public void resource_0_and_1_can_not_be_destroyed()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            p.Invoking( sut => sut.ResTable.Destroy( ctx, 0 ) ).Should().Throw<SqlDetailedException>();
            p.Invoking( sut => sut.ResTable.Destroy( ctx, 1 ) ).Should().Throw<SqlDetailedException>();
        }
    }

    [Test]
    public void CreateResName_raises_an_exception_if_the_resource_is_already_associated_to_a_name_or_the_name_already_exists()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
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
    public void renaming_a_resource_can_be_done_WithChildren_or_only_for_the_resource_itself_by_resId()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            p.ResNameTable.DestroyByResName( ctx, "Test", resNameOnly: false );

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

            p.ResNameTable.Rename( ctx, n1, "Test.MovedTheRootOnly", withChildren: false );
            p.Database.ExecuteScalar( "select ResId from CK.tResName where ResName='Test.MovedTheRootOnly'" )
                    .Should().Be( n1 );
            p.Database.ExecuteScalar( "select ResId from CK.tResName where ResName='Test.-Root-.1'" )
                    .Should().Be( n2 );
            p.Database.ExecuteScalar( "select ResId from CK.tResName where ResName='Test.-Root-.1.1'" )
                .Should().Be( n3 );
        }
    }

    [Test]
    public void renaming_a_resource_can_be_done_WithChildren_or_only_for_the_resource_itself_by_resName()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            p.ResNameTable.DestroyByResName( ctx, "Test", resNameOnly: false );

            int n1 = p.ResNameTable.CreateWithResName( ctx, "Test.Root" );
            int n2 = p.ResNameTable.CreateWithResName( ctx, "Test.Root.1" );
            int n3 = p.ResNameTable.CreateWithResName( ctx, "Test.Root.1.1" );

            p.ResNameTable.Rename( ctx, "Test.Root", "Test.-Root-" );
            p.Database.ExecuteReader( "select * from CK.tResName where ResName like 'Test.Root%'" )
                .Rows.Should().BeEmpty();
            p.Database.ExecuteScalar( "select ResId from CK.tResName where ResName='Test.-Root-'" )
                .Should().Be( n1 );
            p.Database.ExecuteScalar( "select ResId from CK.tResName where ResName='Test.-Root-.1'" )
                .Should().Be( n2 );
            p.Database.ExecuteScalar( "select ResId from CK.tResName where ResName='Test.-Root-.1.1'" )
                .Should().Be( n3 );

            p.ResNameTable.Rename( ctx, "Test.-Root-", "Test.MovedTheRootOnly", withChildren: false );
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
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            var nameRoot = Guid.NewGuid().ToString();

            int n1 = p.ResNameTable.CreateWithResName( ctx, nameRoot + ".Test.Root" );
            int n2 = p.ResNameTable.CreateWithResName( ctx, nameRoot + ".Test.Root.1" );
            int n3 = p.ResNameTable.CreateWithResName( ctx, nameRoot + ".Test.Root.1.1" );
            p.Database.ExecuteScalar( "select count(*) from CK.tResName where ResName like @0+'%'", nameRoot ).Should().Be( 3 );

            p.ResNameTable.DestroyByResName( ctx, nameRoot, withRoot: true, withChildren: true, resNameOnly: false );
            p.Database.ExecuteScalar( "select count(*) from CK.tResName where ResName like @0 + '%'", nameRoot ).Should().Be( 0 );
            p.Database.ExecuteScalar( "select count(*) from CK.tRes where ResId in (@0, @1, @2)", n1, n2, n3 ).Should().Be( 0 );

            // Destroys root, keep children.
            n1 = p.ResNameTable.CreateWithResName( ctx, nameRoot + ".Test.Root" );
            n2 = p.ResNameTable.CreateWithResName( ctx, nameRoot + ".Test.Root.1" );
            n3 = p.ResNameTable.CreateWithResName( ctx, nameRoot + ".Test.Root.1.1" );
            p.Database.ExecuteScalar( "select count(*) from CK.tResName where ResName like @0+'%'", nameRoot ).Should().Be( 3 );

            p.ResNameTable.DestroyByResName( ctx, nameRoot + ".Test.Root", withRoot: true, withChildren: false, resNameOnly: false );
            p.Database.ExecuteScalar( "select count(*) from CK.tResName where ResName like @0+'%'", nameRoot ).Should().Be( 2 );

            p.ResNameTable.DestroyByResName( ctx, nameRoot + ".Test.Root", withRoot: true, withChildren: true, resNameOnly: false );
            p.Database.ExecuteScalar( "select count(*) from CK.tResName where ResName like @0+'%'", nameRoot ).Should().Be( 0 );

            // Destroys children but keep the root.
            n1 = p.ResNameTable.CreateWithResName( ctx, nameRoot + ".Test.Root" );
            n2 = p.ResNameTable.CreateWithResName( ctx, nameRoot + ".Test.Root.1" );
            n3 = p.ResNameTable.CreateWithResName( ctx, nameRoot + ".Test.Root.1.1" );
            p.Database.ExecuteScalar( "select count(*) from CK.tResName where ResName like @0+'%'", nameRoot ).Should().Be( 3 );

            p.ResNameTable.DestroyByResName( ctx, nameRoot + ".Test.Root", withRoot: false, withChildren: true, resNameOnly: false );
            p.Database.ExecuteScalar( "select count(*) from CK.tResName where ResName like @0 + '%'", nameRoot ).Should().Be( 1 );

            p.ResNameTable.DestroyByResName( ctx, nameRoot + ".Test.Root", withRoot: true, withChildren: false, resNameOnly: false );
            p.Database.ExecuteScalar( "select count(*) from CK.tResName where ResName like @0 + '%'", nameRoot ).Should().Be( 0 );

        }
    }
}
