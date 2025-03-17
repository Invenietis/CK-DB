using System.Linq;
using CK.Core;
using CK.SqlServer;
using NUnit.Framework;
using Shouldly;
using CK.Testing;

namespace CK.DB.Culture.Tests;

[TestFixture]
public class ExtendedCultureTests
{
    [Test]
    public void checking_Idx_updates_when_destroying_a_Culture()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            RegisterSampleCultures( p, ctx );
            // Suppressing a culture registered in the middle of the other ones.
            p.DestroyCulture( ctx, 4096 );
            // Without Idx update, registering it again fails.
            RegisterSampleCultures( p, ctx );
        }
    }

    [Test]
    public void reading_CultureData()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            RegisterSampleCultures( p, ctx );
            {
                var c = p.GetCulture( ctx, 10 );
                c.LCID.ShouldBe( 10 );
                c.Name.ShouldBe( "es" );
                c.EnglishName.ShouldBe( "Spanish" );
                c.NativeName.ShouldBe( "español" );
            }
            {
                var c = p.GetCulture( ctx, 1 );
                c.LCID.ShouldBe( 1 );
                c.Name.ShouldBe( "ar" );
                c.EnglishName.ShouldBe( "Arabic" );
                c.NativeName.ShouldBe( "العربية" );
            }
        }
    }

    [Test]
    public void reading_ExtendedCultureData()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            CultureTests.RestoreDatabaseToEnglishAndFrenchOnly( p );
            RegisterSampleCultures( p, ctx );
            var c = p.GetExtendedCulture( ctx, 10 );
            c.XLCID.ShouldBe( 10 );
            c.PrimaryCulture.ShouldNotBeNull();
            c.PrimaryCulture.ShouldBeSameAs( c.Fallbacks[0] );

            c.Fallbacks[0].LCID.ShouldBe( 10 );
            c.Fallbacks[0].Name.ShouldBe( "es" );
            c.Fallbacks[0].EnglishName.ShouldBe( "Spanish" );
            c.Fallbacks[0].NativeName.ShouldBe( "español" );

            c.Fallbacks[1].LCID.ShouldBe( 9 );
            c.Fallbacks[1].Name.ShouldBe( "en" );
            c.Fallbacks[1].EnglishName.ShouldBe( "English" );
            c.Fallbacks[1].NativeName.ShouldBe( "English" );

            c.Fallbacks[2].LCID.ShouldBe( 12 );
            c.Fallbacks[2].Name.ShouldBe( "fr" );
            c.Fallbacks[2].EnglishName.ShouldBe( "French" );
            c.Fallbacks[2].NativeName.ShouldBe( "Français" );

            c.Fallbacks[3].LCID.ShouldBe( 22538 );
            c.Fallbacks[3].Name.ShouldBe( "es-419" );
            c.Fallbacks[3].EnglishName.ShouldBe( "Spanish (Latin America)" );
            c.Fallbacks[3].NativeName.ShouldBe( "español (Latinoamérica)" );

            c.Fallbacks[4].LCID.ShouldBe( 11274 );
            c.Fallbacks[4].Name.ShouldBe( "es-AR" );
            c.Fallbacks[4].EnglishName.ShouldBe( "Spanish (Argentina)" );
            c.Fallbacks[4].NativeName.ShouldBe( "español (Argentina)" );

            c.Fallbacks[5].LCID.ShouldBe( 2060 );
            c.Fallbacks[5].Name.ShouldBe( "fr-BE" );
            c.Fallbacks[5].EnglishName.ShouldBe( "French (Belgium)" );
            c.Fallbacks[5].NativeName.ShouldBe( "français (Belgique)" );

            c.Fallbacks[6].LCID.ShouldBe( 4096 );
            c.Fallbacks[6].Name.ShouldBe( "fr-BF" );
            c.Fallbacks[6].EnglishName.ShouldBe( "French (Burkina Faso)" );
            c.Fallbacks[6].NativeName.ShouldBe( "français (Burkina Faso)" );

            c.Fallbacks[7].LCID.ShouldBe( 4097 );
            c.Fallbacks[7].Name.ShouldBe( "fr-BI" );
            c.Fallbacks[7].EnglishName.ShouldBe( "French (Burundi)" );
            c.Fallbacks[7].NativeName.ShouldBe( "français (Burundi)" );

            c.Fallbacks[8].LCID.ShouldBe( 1 );
            c.Fallbacks[8].Name.ShouldBe( "ar" );
            c.Fallbacks[8].EnglishName.ShouldBe( "Arabic" );
            c.Fallbacks[8].NativeName.ShouldBe( "العربية" );

            c.Fallbacks[9].LCID.ShouldBe( 14337 );
            c.Fallbacks[9].Name.ShouldBe( "ar-AE" );
            c.Fallbacks[9].EnglishName.ShouldBe( "Arabic (United Arab Emirates)" );
            c.Fallbacks[9].NativeName.ShouldBe( "العربية الإمارات العربية المتحدة" );

            c.Fallbacks[10].LCID.ShouldBe( 15361 );
            c.Fallbacks[10].Name.ShouldBe( "ar-BH" );
            c.Fallbacks[10].EnglishName.ShouldBe( "Arabic (Bahrain)" );
            c.Fallbacks[10].NativeName.ShouldBe( "العربية البحرين" );

            c.Fallbacks.Count.ShouldBe( 11 );
        }
    }

    [Test]
    public void reading_CultureDate_or_ExtendedCultureData_with_unexisting_identifier_returns_null()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            p.GetCulture( ctx, 3712 ).ShouldBeNull();
            p.GetExtendedCulture( ctx, 3712 ).ShouldBeNull();
        }
    }

    [Test]
    public void setting_culture_fallbacks()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            CultureTests.RestoreDatabaseToEnglishAndFrenchOnly( p );
            RegisterSpanish( p, ctx );
            RegisterArabic( p, ctx );
            p.GetExtendedCulture( ctx, 10 ).Fallbacks.Select( c => c.LCID )
                .ShouldBe( [10, 9, 12, 1] );
            p.SetLCIDFallbaks( ctx, 10, [10, 12] );
            p.GetExtendedCulture( ctx, 10 ).Fallbacks.Select( c => c.LCID )
                .ShouldBe( [10, 12, 9, 1] );
            p.SetLCIDFallbaks( ctx, 10, [10, 1] );
            p.GetExtendedCulture( ctx, 10 ).Fallbacks.Select( c => c.LCID )
                .ShouldBe( [10, 1, 12, 9] );
            p.SetLCIDFallbaks( ctx, 10, [10, 9, 1, 12] );
            p.GetExtendedCulture( ctx, 10 ).Fallbacks.Select( c => c.LCID )
                .ShouldBe( [10, 9, 1, 12] );
        }
    }

    [Test]
    public void assuming_extended_cultures()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            CultureTests.RestoreDatabaseToEnglishAndFrenchOnly( p );
            RegisterSpanish( p, ctx );
            RegisterArabic( p, ctx );
            p.GetExtendedCulture( ctx, 10 ).Fallbacks.Select( c => c.LCID )
                .ShouldBe( [10, 9, 12, 1] );

            int xlcidSame = p.AssumeXLCID( ctx, [10, 9, 12, 1], allowLCIDMapping: true );
            xlcidSame.ShouldBe( 10, "The primary LCID is okay" );

            int xlcid0 = p.AssumeXLCID( ctx, [10, 9, 12, 1], allowLCIDMapping: false );
            xlcid0.ShouldNotBe( 10 );
            xlcid0.ShouldBeGreaterThan( 0x10000, "A XLCID has been created." );

            int xlcid1 = p.AssumeXLCID( ctx, [10, 1, 9, 12] );
            int xlcid2 = p.AssumeXLCID( ctx, [10, 12, 1, 9] );
            xlcid1.ShouldNotBe( xlcid2 );
            p.GetExtendedCulture( ctx, xlcid1 ).Fallbacks.Select( c => c.LCID )
                .ShouldBe( [10, 1, 9, 12] );
            p.GetExtendedCulture( ctx, xlcid2 ).Fallbacks.Select( c => c.LCID )
                .ShouldBe( [10, 12, 1, 9] );
        }
    }

    [Test]
    public void destroying_culture_updates_all_cultures_fallbacks()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            CultureTests.RestoreDatabaseToEnglishAndFrenchOnly( p );
            RegisterSpanish( p, ctx );
            RegisterArabic( p, ctx );
            p.GetExtendedCulture( ctx, 12 ).Fallbacks.Select( c => c.LCID )
                .ShouldBe( [12, 9, 10, 1] );

            int xlcid12 = p.AssumeXLCID( ctx, [12, 1, 9, 10] );
            p.GetExtendedCulture( ctx, xlcid12 ).Fallbacks.Select( c => c.LCID )
                .ShouldBe( [12, 1, 9, 10] );

            p.DestroyCulture( ctx, 1 );
            p.GetExtendedCulture( ctx, xlcid12 ).Fallbacks.Select( c => c.LCID )
                .ShouldBe( [12, 9, 10] );
            p.GetExtendedCulture( ctx, 12 ).Fallbacks.Select( c => c.LCID )
                .ShouldBe( [12, 9, 10] );
        }
    }

    [Test]
    public void destroying_LCID_destroys_XLCID_with_the_primary_LCID()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            CultureTests.RestoreDatabaseToEnglishAndFrenchOnly( p );
            RegisterSpanish( p, ctx );
            RegisterArabic( p, ctx );
            int xlcid1a = p.AssumeXLCID( ctx, [1, 9, 10, 12] );
            int xlcid1b = p.AssumeXLCID( ctx, [1, 10, 9, 12] );
            p.GetExtendedCulture( ctx, xlcid1a ).ShouldNotBeNull();
            p.GetExtendedCulture( ctx, xlcid1b ).ShouldNotBeNull();

            p.DestroyCulture( ctx, 1 );
            p.GetExtendedCulture( ctx, xlcid1a ).ShouldBeNull();
            p.GetExtendedCulture( ctx, xlcid1b ).ShouldBeNull();
        }
    }

    [Test]
    public void XLCID_are_never_reused()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            RegisterSpanish( p, ctx );
            RegisterArabic( p, ctx );
            int xlcid = p.AssumeXLCID( ctx, [1, 9, 10, 12] );
            p.DestroyCulture( ctx, xlcid );
            int xlcid2 = p.AssumeXLCID( ctx, [1, 9, 10, 12] );
            xlcid2.ShouldBeGreaterThan( xlcid );
        }
    }

    public static void RegisterSampleCultures( Package p, SqlStandardCallContext ctx )
    {
        //  10      es      Spanish                        español 
        //  22538   es-419  Spanish (Latin America)        español (Latinoamérica)
        //  11274   es-AR   Spanish (Argentina)            español (Argentina)

        //  2060    fr-BE   French (Belgium)               français (Belgique)                                
        //  4096    fr-BF   French (Burkina Faso)          français (Burkina Faso)                            

        // We'll use 4097 for French (Burundi)
        //  4096    fr-BI   French (Burundi)               français (Burundi)                                 

        //  1       ar      Arabic                         العربية
        //  14337   ar-AE   Arabic (United Arab Emirates)  العربية الإمارات العربية المتحدة 
        //  15361   ar-BH   Arabic (Bahrain)               العربية البحرين 

        RegisterSpanish( p, ctx );
        p.Register( ctx, 22538, "es-419", "Spanish (Latin America)", "español (Latinoamérica)" );
        p.Register( ctx, 11274, "es-AR", "Spanish (Argentina)", "español (Argentina)" );

        p.Register( ctx, 2060, "fr-BE", "French (Belgium)", "français (Belgique)" );
        p.Register( ctx, 4096, "fr-BF", "French (Burkina Faso)", "français (Burkina Faso)" );
        p.Register( ctx, 4097, "fr-BI", "French (Burundi)", "français (Burundi)" );

        RegisterArabic( p, ctx );
        p.Register( ctx, 14337, "ar-AE", "Arabic (United Arab Emirates)", "العربية الإمارات العربية المتحدة" );
        p.Register( ctx, 15361, "ar-BH", "Arabic (Bahrain)", "العربية البحرين" );
    }

    /// <summary>
    /// Registers Spanish (LCID = 1).
    /// </summary>
    /// <param name="p">Cuture package.</param>
    /// <param name="ctx">Call context to use.</param>
    public static void RegisterArabic( Package p, SqlStandardCallContext ctx )
    {
        p.Register( ctx, 1, "ar", "Arabic", "العربية" );
    }

    /// <summary>
    /// Registers Spanish (LCID = 10).
    /// </summary>
    /// <param name="p">Cuture package.</param>
    /// <param name="ctx">Call context to use.</param>
    public static void RegisterSpanish( Package p, SqlStandardCallContext ctx )
    {
        p.Register( ctx, 10, "es", "Spanish", "español" );
    }

    /// <summary>
    /// Registers German (LCID = 7).
    /// </summary>
    /// <param name="p">Cuture package.</param>
    /// <param name="ctx">Call context to use.</param>
    public static void RegisterGerman( Package p, SqlStandardCallContext ctx )
    {
        p.Register( ctx, 7, "de", "German", "Deutsch" );
    }
}
