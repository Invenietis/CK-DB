using CK.Core;
using CK.SqlServer;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CK.Testing.DBSetupTestHelper;

namespace CK.DB.Res.MCResHtml.Tests
{
    [TestFixture]
    public class MCResHtmlTests
    {
        [Test]
        public void fallbaks_between_french_and_english_cultures()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int noValuesId, enId, frId, bothId;
                AssumeFallbackTestEnglishAndFrenchResources( p, ctx, out noValuesId, out enId, out frId, out bothId );

                CheckString( p, noValuesId, 9, DBNull.Value, DBNull.Value );
                CheckString( p, noValuesId, 12, DBNull.Value, DBNull.Value );

                CheckString( p, enId, 9, "Only in English.", 9 );
                CheckString( p, enId, 12, "Only in English.", 9 );

                CheckString( p, frId, 9, "Seulement en Français.", 12 );
                CheckString( p, frId, 12, "Seulement en Français.", 12 );

                CheckString( p, bothId, 9, "English (and French).", 9 );
                CheckString( p, bothId, 12, "Français (et Anglais).", 12 );
            }
        }

        [Test]
        public void fallbaks_between_french_and_english_and_german_cultures()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {

                int noValuesId, enId, frId, bothId;
                AssumeFallbackTestEnglishAndFrenchResources( p, ctx, out noValuesId, out enId, out frId, out bothId );
                int deId = p.ResTable.Create( ctx );
                int allId = p.ResTable.Create( ctx );

                Culture.Tests.ExtendedCultureTests.RegisterGerman( p.Culture, ctx );

                p.MCResHtmlTable.SetHtml( ctx, deId, 7, "Nur in deutscher Sprache." );
                p.MCResHtmlTable.SetHtml( ctx, allId, 12, "Français (et Anglais et Allemand)." );
                p.MCResHtmlTable.SetHtml( ctx, allId, 9, "English (and French and German)." );
                p.MCResHtmlTable.SetHtml( ctx, allId, 7, "Deutsch (und Englisch und Französisch)." );

                CheckString( p, noValuesId, 9, DBNull.Value, DBNull.Value );
                CheckString( p, noValuesId, 12, DBNull.Value, DBNull.Value );
                CheckString( p, noValuesId, 7, DBNull.Value, DBNull.Value );

                CheckString( p, enId, 9, "Only in English.", 9 );
                CheckString( p, enId, 12, "Only in English.", 9 );
                CheckString( p, enId, 7, "Only in English.", 9 );

                CheckString( p, frId, 9, "Seulement en Français.", 12 );
                CheckString( p, frId, 12, "Seulement en Français.", 12 );
                CheckString( p, frId, 7, "Seulement en Français.", 12 );

                CheckString( p, bothId, 9, "English (and French).", 9 );
                CheckString( p, bothId, 12, "Français (et Anglais).", 12 );
                CheckString( p, bothId, 7, "English (and French).", 9 );

                CheckString( p, allId, 9, "English (and French and German).", 9 );
                CheckString( p, allId, 12, "Français (et Anglais et Allemand).", 12 );
                CheckString( p, allId, 7, "Deutsch (und Englisch und Französisch).", 7 );
            }
        }

        static void AssumeFallbackTestEnglishAndFrenchResources( Package p, SqlStandardCallContext ctx, out int noValuesId, out int enId, out int frId, out int bothId )
        {
            noValuesId = p.ResTable.Create( ctx );
            enId = p.ResTable.Create( ctx );
            p.MCResHtmlTable.SetHtml( ctx, enId, 9, "Only in English." );
            frId = p.ResTable.Create( ctx );
            p.MCResHtmlTable.SetHtml( ctx, frId, 12, "Seulement en Français." );
            bothId = p.ResTable.Create( ctx );
            p.MCResHtmlTable.SetHtml( ctx, bothId, 9, "English (and French)." );
            p.MCResHtmlTable.SetHtml( ctx, bothId, 12, "Français (et Anglais)." );
        }

        static void CheckString( Package p, int resId, int lcid, object expectedValue, object expectedLCID )
        {
            p.Database.ExecuteScalar( "select Value from CK.vMCResHtml where ResId=@0 and XLCID = @1", resId, lcid )
                .Should().Be( expectedValue );
            p.Database.ExecuteScalar( "select LCID from CK.vMCResHtml where ResId=@0 and XLCID = @1", resId, lcid )
                .Should().Be( expectedLCID );
        }

        [Test]
        public void setting_and_clearing_string_values_can_use_XLCID()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                Culture.Tests.ExtendedCultureTests.RegisterSpanish( p.Culture, ctx );
                Culture.Tests.ExtendedCultureTests.RegisterArabic( p.Culture, ctx );
                int xlcid1 = p.Culture.AssumeXLCID( ctx, new[] { 1, 9, 10, 12 } );
                xlcid1.Should().BeGreaterThan( 0xFFFF );
                int xlcid9 = p.Culture.AssumeXLCID( ctx, new[] { 9, 1, 12, 10 } );
                xlcid9.Should().BeGreaterThan( 0xFFFF );
                int xlcid10 = p.Culture.AssumeXLCID( ctx, new[] { 10, 1, 9, 12 } );
                xlcid10.Should().BeGreaterThan( 0xFFFF );
                int resId = p.ResTable.Create( ctx );

                CheckString( p, resId, 1, DBNull.Value, DBNull.Value );
                CheckString( p, resId, 9, DBNull.Value, DBNull.Value );
                CheckString( p, resId, 10, DBNull.Value, DBNull.Value );
                CheckString( p, resId, 12, DBNull.Value, DBNull.Value );
                CheckString( p, resId, xlcid1, DBNull.Value, DBNull.Value );
                CheckString( p, resId, xlcid9, DBNull.Value, DBNull.Value );
                CheckString( p, resId, xlcid10, DBNull.Value, DBNull.Value );

                p.MCResHtmlTable.SetHtml( ctx, resId, xlcid1, "الأزمة في مصر" );
                CheckString( p, resId, 1, "الأزمة في مصر", 1 );
                CheckString( p, resId, 9, "الأزمة في مصر", 1 );
                CheckString( p, resId, 10, "الأزمة في مصر", 1 );
                CheckString( p, resId, 12, "الأزمة في مصر", 1 );
                CheckString( p, resId, xlcid1, "الأزمة في مصر", 1 );
                CheckString( p, resId, xlcid9, "الأزمة في مصر", 1 );
                CheckString( p, resId, xlcid10, "الأزمة في مصر", 1 );

                p.MCResHtmlTable.SetHtml( ctx, resId, xlcid10, "¡Hola! (España)" );
                CheckString( p, resId, 1, "الأزمة في مصر", 1 );
                CheckString( p, resId, 9, "¡Hola! (España)", 10 );
                CheckString( p, resId, 10, "¡Hola! (España)", 10 );
                CheckString( p, resId, 12, "¡Hola! (España)", 10 );
                CheckString( p, resId, xlcid1, "الأزمة في مصر", 1 );
                CheckString( p, resId, xlcid9, "الأزمة في مصر", 1 );
                CheckString( p, resId, xlcid10, "¡Hola! (España)", 10 );

                p.MCResHtmlTable.SetHtml( ctx, resId, 12, "Liberté!" );
                CheckString( p, resId, 1, "الأزمة في مصر", 1 );
                CheckString( p, resId, 9, "Liberté!", 12 );
                CheckString( p, resId, 10, "¡Hola! (España)", 10 );
                CheckString( p, resId, 12, "Liberté!", 12 );
                CheckString( p, resId, xlcid1, "الأزمة في مصر", 1 );
                CheckString( p, resId, xlcid9, "الأزمة في مصر", 1 );
                CheckString( p, resId, xlcid10, "¡Hola! (España)", 10 );

                p.MCResHtmlTable.SetHtml( ctx, resId, xlcid1, null );
                CheckString( p, resId, 1, "Liberté!", 12 );
                CheckString( p, resId, 9, "Liberté!", 12 );
                CheckString( p, resId, 10, "¡Hola! (España)", 10 );
                CheckString( p, resId, 12, "Liberté!", 12 );
                CheckString( p, resId, xlcid1, "¡Hola! (España)", 10 );
                CheckString( p, resId, xlcid9, "Liberté!", 12 );
                CheckString( p, resId, xlcid10, "¡Hola! (España)", 10 );

                p.MCResHtmlTable.SetHtml( ctx, resId, xlcid10, null );
                CheckString( p, resId, 1, "Liberté!", 12 );
                CheckString( p, resId, 9, "Liberté!", 12 );
                CheckString( p, resId, 10, "Liberté!", 12 );
                CheckString( p, resId, 12, "Liberté!", 12 );
                CheckString( p, resId, xlcid1, "Liberté!", 12 );
                CheckString( p, resId, xlcid9, "Liberté!", 12 );
                CheckString( p, resId, xlcid10, "Liberté!", 12 );


                Assert.DoesNotThrow( () => p.ResTable.Destroy( ctx, resId ) );

                CheckString( p, resId, 1, null, null );
                CheckString( p, resId, 9, null, null );
                CheckString( p, resId, 10, null, null );
                CheckString( p, resId, 12, null, null );
                CheckString( p, resId, xlcid1, null, null );
                CheckString( p, resId, xlcid9, null, null );
                CheckString( p, resId, xlcid10, null, null );

            }
        }

        [Test]
        public void destroying_the_resource_destroys_the_string_values()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                int noValuesId, enId, frId, bothId;
                AssumeFallbackTestEnglishAndFrenchResources( p, ctx, out noValuesId, out enId, out frId, out bothId );

                p.ResTable.Destroy( ctx, noValuesId );
                p.ResTable.Destroy( ctx, enId );
                p.ResTable.Destroy( ctx, frId );
                p.ResTable.Destroy( ctx, bothId );
                p.Database.ExecuteReader( "select Value from CK.vMCResHtml where ResId=@0", bothId )
                    .Rows.Should().BeEmpty();

                Assert.DoesNotThrow( () => p.MCResHtmlTable.SetHtml( ctx, bothId, 9, null ) );
            }

        }

    }
}
