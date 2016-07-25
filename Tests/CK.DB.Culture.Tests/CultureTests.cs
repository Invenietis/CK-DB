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

namespace CK.DB.Culture.Tests
{
    [TestFixture]
    public class LCIDTests
    {
        [Test]
        public void French_and_English_are_defined_by_default_but_may_be_updated()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {

                p.Database.AssertScalarEquals( 2, "select count(*) from CK.tLCID where LCID in(9,12)" )
                          .AssertRawExecute( 1, "update CK.tLCID set Name = @0, NativeName = @0, EnglishName = @0 where LCID = 9", "EN-ALTERED" )
                          .AssertRawExecute( 1, "update CK.tLCID set Name = @0, NativeName = @0, EnglishName = @0 where LCID = 12", "FR-ALTERED" );

                p.LCIDTable.RegisterCulture( ctx, 9, "en", "English", "English", 0 );
                p.Database.AssertScalarEquals( "en", "select Name from CK.tLCID where LCID = 9" )
                          .AssertScalarEquals( "English", "select NativeName from CK.tLCID where LCID = 9" )
                          .AssertScalarEquals( "English", "select EnglishName from CK.tLCID where LCID = 9" )
                          .AssertScalarEquals( "FR-ALTERED", "select Name from CK.tLCID where LCID = 12" );

                p.LCIDTable.RegisterCulture( ctx, 12, "fr", "French", "Français", 0 );
                p.Database.AssertScalarEquals( "fr", "select Name from CK.tLCID where LCID = 12" )
                          .AssertScalarEquals( "Français", "select NativeName from CK.tLCID where LCID = 12" )
                          .AssertScalarEquals( "French", "select EnglishName from CK.tLCID where LCID = 12" );
            }
        }

        [Test]
        public void registering_a_culture_updates_all_fallbacks()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                //  44    az            Azerbaijani                         Azərbaycan­ılı
                //  29740 az-Cyrl       Azerbaijani(Cyrillic)               Азәрбајҹан дили
                //  2092  az-Cyrl-AZ    Azerbaijani(Cyrillic, Azerbaijan)   Азәрбајҹан дили (Азәрбајҹан)

                p.LCIDTable.RegisterCulture( ctx, 44, "az", "Azerbaijani", "Azərbaycan­ılı", 0 );
                p.LCIDTable.RegisterCulture( ctx, 29740, "az-Cyrl", "Azerbaijani(Cyrillic)", "Азәрбајҹан дили", 44 );
                p.LCIDTable.RegisterCulture( ctx, 2092, "az-Cyrl-AZ", "Azerbaijani(Cyrillic, Azerbaijan)", "Азәрбајҹан дили (Азәрбајҹан)", 29740 );

                p.Database.AssertScalarEquals( "9,12,44,29740,2092", "select FallbacksLCID from CK.vXLCID where XLCID = 9" )
                          .AssertScalarEquals( "12,9,44,29740,2092", "select FallbacksLCID from CK.vXLCID where XLCID = 12" );

                p.LCIDTable.DestroyCulture( ctx, 2092 );
                p.Database.AssertScalarEquals( "9,12,44,29740", "select FallbacksLCID from CK.vXLCID where XLCID = 9" )
                          .AssertScalarEquals( "12,9,44,29740", "select FallbacksLCID from CK.vXLCID where XLCID = 12" );

                p.LCIDTable.DestroyCulture( ctx, 29740 );
                p.LCIDTable.DestroyCulture( ctx, 44 );

                p.Database.AssertScalarEquals( "9,12", "select FallbacksLCID from CK.vXLCID where XLCID = 9" )
                          .AssertScalarEquals( "12,9", "select FallbacksLCID from CK.vXLCID where XLCID = 12" );
            }
        }

        [Test]
        [Explicit]
        public void display_all_available_cultures()
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
            Console.WriteLine( "LCID, Name,      DisplayName,                EnglishName,              Native,            IsNeutralCulture" );
            foreach( var c in lcids )
            {
                Console.WriteLine( "{0,-5} {1,-15} {2,-50} {3,-50} {4,-50} {5,-15}", c.LCID, c.Name, c.DisplayName, c.EnglishName, c.NativeName, c.IsNeutralCulture );
            }

        }
    }

}
