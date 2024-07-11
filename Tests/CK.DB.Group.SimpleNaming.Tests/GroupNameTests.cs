using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.Group.SimpleNaming.Tests
{
    [TestFixture]
    public class GroupNameTests
    {
        [Test]
        public void a_group_can_be_renamed()
        {
            var map = SharedEngine.Map;
            var g = map.StObjs.Obtain<GroupTable>();
            var gN = map.StObjs.Obtain<SimpleNaming.Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                string uniquifierName = Guid.NewGuid().ToString();
                int groupId = g.CreateGroup( ctx, 1 );
                string name = gN.GroupRename( ctx, 1, groupId, uniquifierName + "Group" );
                name.Should().Be( uniquifierName + "Group" );
                g.Database.ExecuteScalar( "select GroupName from CK.tGroup where GroupId = @0", groupId )
                    .Should().Be( name );
                name = gN.GroupRename( ctx, 1, groupId, uniquifierName + "Another Group" );
                name.Should().Be( uniquifierName + "Another Group" );
                g.Database.ExecuteScalar( "select GroupName from CK.tGroup where GroupId = @0", groupId )
                    .Should().Be( name );
                g.DestroyGroup( ctx, 1, groupId );
                g.Database.ExecuteReader( "select * from CK.tGroup where GroupId = @0", groupId )
                    .Rows.Should().BeEmpty();
            }
        }

        [Test]
        public void renaming_a_group_with_an_already_conflicting_name_finds_the_hole()
        {
            var map = SharedEngine.Map;
            var g = map.StObjs.Obtain<GroupTable>();
            var gN = map.StObjs.Obtain<SimpleNaming.Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                string uniquifierName = Guid.NewGuid().ToString();
                string name;
                int g1 = g.CreateGroup( ctx, 1 );
                name = gN.GroupRename( ctx, 1, g1, uniquifierName );
                name.Should().Be( uniquifierName );
                int g2 = g.CreateGroup( ctx, 1 );
                name = gN.GroupRename( ctx, 1, g2, uniquifierName );
                name.Should().Be( uniquifierName + " (1)" );
                int g3 = g.CreateGroup( ctx, 1 );
                name = gN.GroupRename( ctx, 1, g3, uniquifierName );
                name.Should().Be( uniquifierName + " (2)" );

                name = gN.GroupRename( ctx, 1, g3, uniquifierName );
                name.Should().Be( uniquifierName + " (2)", "No change: found the (2) again." );

                g.DestroyGroup( ctx, 1, g2 );
                name = gN.GroupRename( ctx, 1, g3, uniquifierName );
                name.Should().Be( uniquifierName + " (1)", "The (1) is found." );

                g.DestroyGroup( ctx, 1, g1 );
                g.DestroyGroup( ctx, 1, g3 );
            }
        }

        [Test]
        public void group_names_are_unique_and_clash_are_atomatically_handled()
        {
            var map = SharedEngine.Map;
            var g = map.StObjs.Obtain<GroupTable>();
            var gN = map.StObjs.Obtain<SimpleNaming.Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                string[] names = Enumerable.Range( 0, 8 ).Select( i => Guid.NewGuid().ToString() + " - " + i ).ToArray();
                int[] groups = new int[names.Length];
                string newName;
                for( int i = 0; i < names.Length; ++i )
                {
                    groups[i] = g.CreateGroup( ctx, 1 );
                    newName = gN.GroupRename( ctx, 1, groups[i], names[i] );
                    newName.Should().Be( names[i] );
                }

                // Renaming all of them like the first one:
                for( int i = 1; i < names.Length; ++i )
                {
                    newName = gN.GroupRename( ctx, 1, groups[i], names[0] );
                    newName.Should().Be( names[0] + " (" + i + ")" );
                }

                // Renaming the first one with no change:
                newName = gN.GroupRename( ctx, 1, groups[0], names[0] );
                newName.Should().Be( names[0] );

                // Renaming all of them in the opposite order: no clash.
                for( int i = 0; i < names.Length; ++i )
                {
                    newName = gN.GroupRename( ctx, 1, groups[i], names[names.Length - i - 1] );
                    newName.Should().Be( names[names.Length - i - 1] );
                }
            }
        }

        [Test]
        public void new_group_name_can_be_checked()
        {
            var map = SharedEngine.Map;
            var g = map.StObjs.Obtain<GroupTable>();
            var gN = map.StObjs.Obtain<SimpleNaming.Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                string theGroupName = Guid.NewGuid().ToString();
                int groupId = g.CreateGroup( ctx, 1 );
                gN.GroupRename( ctx, 1, groupId, theGroupName );

                string newName;
                newName = gN.CheckUniqueNameForNewGroup( ctx, theGroupName );
                newName.Should().Be( theGroupName + " (1)" );

                newName = gN.CheckUniqueName( ctx, groupId, theGroupName );
                newName.Should().Be( theGroupName );

                g.DestroyGroup( ctx, 1, groupId );

                newName = gN.CheckUniqueNameForNewGroup( ctx, theGroupName );
                newName.Should().Be( theGroupName );
            }
        }

        [Test]
        public void group_name_is_nvarchar_128()
        {
            var map = SharedEngine.Map;
            var g = map.StObjs.Obtain<GroupTable>();
            var gN = map.StObjs.Obtain<SimpleNaming.Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                string theGroupName = new string( '-', 127 ) + 'X';
                string theConflictNameRoot = new string( '-', 123 );

                int groupId = g.CreateGroup( ctx, 1 );
                gN.GroupRename( ctx, 1, groupId, theGroupName );

                string newName;
                newName = gN.CheckUniqueNameForNewGroup( ctx, theGroupName );
                newName.Should().Be( theConflictNameRoot + " (1)" );

                g.DestroyGroup( ctx, 1, groupId );

                newName = gN.CheckUniqueNameForNewGroup( ctx, theGroupName );
                newName.Should().Be( theGroupName );
            }
        }

        [Test]
        public void when_there_is_no_more_room_for_rename_checking_group_name_returns_null()
        {
            var map = SharedEngine.Map;
            var g = map.StObjs.Obtain<GroupTable>();
            var gN = map.StObjs.Obtain<SimpleNaming.Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                string theGroupName = Guid.NewGuid().ToString();
                int idMain = g.CreateGroup( ctx, 1 );
                gN.GroupRename( ctx, 1, idMain, theGroupName ).Should().Be( theGroupName );
                var others = new List<int>();
                for( int i = 0; i < gN.MaxClashNumber; ++i )
                {
                    int id = g.CreateGroup( ctx, 1 );
                    string corrected = gN.GroupRename( ctx, 1, id, theGroupName );
                    corrected.Should().NotBe( theGroupName );
                    others.Add( id );
                }
                string clash = gN.CheckUniqueNameForNewGroup( ctx, theGroupName );
                clash.Should().BeNull();
                int idTooMuch = g.CreateGroup( ctx, 1 );
                gN.Invoking( sut => sut.GroupRename( ctx, 1, idTooMuch, theGroupName ) ).Should().Throw<SqlDetailedException>();

            }
        }
    }
}
