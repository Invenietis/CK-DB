using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CK.Core;
using CK.SqlServer;
using NUnit.Framework;
using System.Globalization;
using FluentAssertions;

namespace CK.DB.Culture.Tests
{
    [TestFixture]
    public class CultureTests
    {
        [Test]
        public void by_default_French_and_English_are_defined_but_may_be_updated()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {

                p.Database.ExecuteScalar( "select count(*) from CK.tLCID where LCID in(9,12)" )
                    .Should().Be( 2 );
                p.Database.ExecuteNonQuery( "update CK.tLCID set Name = @0, NativeName = @0, EnglishName = @0 where LCID = 9", "EN-ALTERED" )
                    .Should().Be( 1 );
                p.Database.ExecuteNonQuery( "update CK.tLCID set Name = @0, NativeName = @0, EnglishName = @0 where LCID = 12", "FR-ALTERED" )
                    .Should().Be( 1 );

                p.Register( ctx, 9, "en", "English", "English" );
                p.Database.ExecuteScalar( "select Name from CK.tLCID where LCID = 9" )
                    .Should().Be( "en" );
                p.Database.ExecuteScalar( "select NativeName from CK.tLCID where LCID = 9" )
                    .Should().Be( "English" );
                p.Database.ExecuteScalar( "select EnglishName from CK.tLCID where LCID = 9" )
                    .Should().Be( "English" );
                p.Database.ExecuteScalar( "select Name from CK.tLCID where LCID = 12" )
                    .Should().Be( "FR-ALTERED" );

                p.Register( ctx, 12, "fr", "French", "Français" );
                p.Database.ExecuteScalar( "select Name from CK.tLCID where LCID = 12" )
                    .Should().Be( "fr" );
                p.Database.ExecuteScalar( "select NativeName from CK.tLCID where LCID = 12" )
                    .Should().Be( "Français" );
                p.Database.ExecuteScalar( "select EnglishName from CK.tLCID where LCID = 12" )
                    .Should().Be( "French" );
            }
        }

        [Test]
        public void registering_a_culture_updates_all_fallbacks()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Culture.Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                // Removes all cultures except 9 and 12.
                RestoreDatabaseToEnglishAndFrenchOnly( p );

                //  44    az            Azerbaijani                          Azərbaycan­ılı
                //  29740 az-Cyrl       Azerbaijani (Cyrillic)               Азәрбајҹан дили
                //  2092  az-Cyrl-AZ    Azerbaijani (Cyrillic, Azerbaijan)   Азәрбајҹан дили (Азәрбајҹан)

                p.Register( ctx, 44, "az", "Azerbaijani", "Azərbaycan­ılı" );
                p.Register( ctx, 29740, "az-Cyrl", "Azerbaijani(Cyrillic)", "Азәрбајҹан дили" );
                p.Register( ctx, 2092, "az-Cyrl-AZ", "Azerbaijani(Cyrillic, Azerbaijan)", "Азәрбајҹан дили (Азәрбајҹан)" );

                p.Database.ExecuteScalar( "select FallbacksLCID from CK.vXLCID where XLCID = 9" )
                    .Should().Be( "9,12,44,29740,2092" );
                p.Database.ExecuteScalar( "select FallbacksLCID from CK.vXLCID where XLCID = 12" )
                    .Should().Be( "12,9,44,29740,2092" );

                p.DestroyCulture( ctx, 2092 );
                p.Database.ExecuteScalar( "select FallbacksLCID from CK.vXLCID where XLCID = 9" )
                    .Should().Be( "9,12,44,29740" );
                p.Database.ExecuteScalar( "select FallbacksLCID from CK.vXLCID where XLCID = 12" )
                    .Should().Be( "12,9,44,29740" );

                p.DestroyCulture( ctx, 29740 );
                p.DestroyCulture( ctx, 44 );

                p.Database.ExecuteScalar( "select FallbacksLCID from CK.vXLCID where XLCID = 9" )
                    .Should().Be( "9,12" );
                p.Database.ExecuteScalar( "select FallbacksLCID from CK.vXLCID where XLCID = 12" )
                    .Should().Be( "12,9" );
            }
        }

        public static void RestoreDatabaseToEnglishAndFrenchOnly( Package p )
        {
            p.Database.ExecuteNonQuery( @"while 1 = 1
                                        begin
	                                        declare @XLCID int = null;
	                                        select top 1 @XLCID = XLCID from CK.tXLCID where XLCID not in (0,9,12);
	                                        if @XLCID is null break;
	                                        exec CK.sCultureDestroy @XLCID;
                                        end" );
        }

        [Test]
        public void LCID_must_be_greater_than_0_and_less_than_0xFFFF()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                Assert.Throws<SqlDetailedException>( () => p.Register( ctx, 0, "xx", "XXX", "XXX" ) );
                Assert.Throws<SqlDetailedException>( () => p.Register( ctx, 0xFFFFF, "xx", "XXX", "XXX" ) );
            }
        }

        [Test]
        [Explicit]
        public void display_all_available_Framework_cultures()
        {
            var lcids = CultureInfo.GetCultures( CultureTypes.AllCultures );
            Console.WriteLine( "MinLCID= {0}, MaxLCID={1}, Name.Max={2}, DisplayName.Max={3}, EnglishName.Max={4}, NativeName.Max={5}, HasCommaInNames={6}",
                                lcids.Select( c => c.LCID ).Min(),
                                lcids.Select( c => c.LCID ).Max(),
                                lcids.Select( c => c.Name.Length ).Max(),
                                lcids.Select( c => c.DisplayName.Length ).Max(),
                                lcids.Select( c => c.EnglishName.Length ).Max(),
                                lcids.Select( c => c.NativeName.Length ).Max(),
                                lcids.Select( c => c.Name + c.EnglishName + c.NativeName ).Any( s => s.Contains( ',' ) ) );
            Console.WriteLine( "LCID,  Parent,   EnglishName,       Native" );
            foreach( var c in lcids )
            {
                Console.WriteLine( "{0,-6} {1,-6} {2,-50} {3,-50}", c.LCID, c.Parent?.LCID, c.EnglishName, c.NativeName );
            }

        }
    }

}
