using CK.Core;
using CK.SqlServer;
using CK.Testing;
using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.Auth.AuthScope.Tests
{
    [TestFixture]
    public class AuthScopeSetTests
    {
        [Test]
        public async Task creating_simple_scope_set_Async()
        {
            var scopes = SharedEngine.Map.StObjs.Obtain<AuthScopeSetTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var id = await scopes.CreateScopeSetAsync( ctx, 1, "openid profile" );
                Assert.That( id, Is.GreaterThan( 0 ) );
                scopes.Database.ExecuteScalar( $"select count(*) from CK.tAuthScopeSetContent where ScopeSetId = {id}" )
                    .Should().Be( 2 );
                await scopes.DestroyScopeSetAsync( ctx, 1, id );
                scopes.Database.ExecuteReader( $"select * from CK.tAuthScopeSetContent where ScopeSetId = {id}" )
                    .Rows.Should().BeEmpty();
            }
        }

        [Test]
        public async Task setting_scopes_on_zero_ScopeSetId_is_an_error_Async()
        {
            var scopes = SharedEngine.Map.StObjs.Obtain<AuthScopeSetTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                try
                {
                    await scopes.SetScopesAsync( ctx, 1, 0, "openid profile", false );
                    Assert.Fail( "Modifying the zero ScopeSetId is an error." );
                }
                catch( SqlDetailedException )
                {
                }
            }
        }

        [Test]
        public async Task adding_and_removing_scopes_via_raw_strings_Async()
        {
            var scopes = SharedEngine.Map.StObjs.Obtain<AuthScopeSetTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var id = await scopes.CreateScopeSetAsync( ctx, 1, "   profile   openid  " );
                scopes.Database.ExecuteScalar( $"select ScopesWithStatus from CK.vAuthScopeSet where ScopeSetId = {id}" )
                    .Should().Be( "[W]openid [W]profile" );

                await scopes.RemoveScopesAsync( ctx, 1, id, ScopeWARStatus.Waiting );
                scopes.Database.ExecuteScalar( $"select ScopesWithStatus from CK.vAuthScopeSet where ScopeSetId = {id}" )
                    .Should().Be( string.Empty );

                await scopes.AddOrUpdateScopesAsync( ctx, 1, id, "Z Y X A", true, ScopeWARStatus.Waiting );
                scopes.Database.ExecuteScalar( $"select ScopesWithStatus from CK.vAuthScopeSet where ScopeSetId = {id}" )
                    .Should().Be( "[W]A [W]X [W]Y [W]Z" );

                await scopes.RemoveScopesAsync( ctx, 1, id, "A Y Z", false );
                scopes.Database.ExecuteScalar( $"select ScopesWithStatus from CK.vAuthScopeSet where ScopeSetId = {id}" )
                    .Should().Be( "[W]X" );

                await scopes.SetScopesAsync( ctx, 1, id, "a b c d a b", false, ScopeWARStatus.Accepted );
                scopes.Database.ExecuteScalar( $"select ScopesWithStatus from CK.vAuthScopeSet where ScopeSetId = {id}" )
                    .Should().Be( "[A]a [A]b [A]c [A]d" );

                await scopes.AddOrUpdateScopesAsync( ctx, 1, id, "a d z", false, ScopeWARStatus.Rejected );
                scopes.Database.ExecuteScalar( $"select ScopesWithStatus from CK.vAuthScopeSet where ScopeSetId = {id}" )
                    .Should().Be( "[R]a [A]b [A]c [R]d [R]z" );

                await scopes.RemoveScopesAsync( ctx, 1, id, "a b c z", false, ScopeWARStatus.Rejected );
                scopes.Database.ExecuteScalar( $"select ScopesWithStatus from CK.vAuthScopeSet where ScopeSetId = {id}" )
                    .Should().Be( "[A]b [A]c [R]d" );

                await scopes.DestroyScopeSetAsync( ctx, 1, id );
            }
        }

        [Test]
        public async Task AuthScopeSet_manipulation_Async()
        {
            var scopes = SharedEngine.Map.StObjs.Obtain<AuthScopeSetTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var set = new AuthScopeSet( new[] {
                    new AuthScopeItem( "A", ScopeWARStatus.Accepted ),
                    new AuthScopeItem( "B", ScopeWARStatus.Waiting ),
                    new AuthScopeItem( "C", ScopeWARStatus.Rejected )
                } );

                var id = await scopes.CreateScopeSetAsync( ctx, 1, set.Scopes );
                scopes.Database.ExecuteScalar( $"select ScopesWithStatus from CK.vAuthScopeSet where ScopeSetId = {id}" )
                    .Should().Be( "[A]A [W]B [R]C" );

                var readSet = await scopes.ReadAuthScopeSetAsync( ctx, id );
                readSet.ScopeSetId.Should().Be( id );
                readSet.ToString().Should().Be( "[A]A [W]B [R]C" );

                set.ScopeSetId = id;
                set.Add( new AuthScopeItem( "B", ScopeWARStatus.Accepted ) );
                set.Add( new AuthScopeItem( "D", ScopeWARStatus.Waiting ) );
                await scopes.AddOrUpdateScopesAsync( ctx, 1, set );
                readSet = await scopes.ReadAuthScopeSetAsync( ctx, id );
                readSet.ToString().Should().Be( "[A]A [A]B [R]C [W]D" );

                set.Remove( "B" );
                set.Remove( "C" );
                set.Add( new AuthScopeItem( "E", ScopeWARStatus.Accepted ) );
                await scopes.AddOrUpdateScopesAsync( ctx, 1, set );
                readSet = await scopes.ReadAuthScopeSetAsync( ctx, id );
                readSet.ToString().Should().Be( "[A]A [A]B [R]C [W]D [A]E" );

                set.Remove( "E" );
                await scopes.SetScopesAsync( ctx, 1, set );
                readSet = await scopes.ReadAuthScopeSetAsync( ctx, id );
                readSet.ToString().Should().Be( "[A]A [W]D" );

                await scopes.DestroyScopeSetAsync( ctx, 1, id );
            }
        }


    }
}
