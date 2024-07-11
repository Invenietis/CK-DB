using System.Linq;
using CK.Core;
using CK.SqlServer;
using NUnit.Framework;
using FluentAssertions;
using CK.Testing;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.Culture.Tests
{
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
                    c.LCID.Should().Be( 10 );
                    c.Name.Should().Be( "es" );
                    c.EnglishName.Should().Be( "Spanish" );
                    c.NativeName.Should().Be( "español" );
                }
                {
                    var c = p.GetCulture( ctx, 1 );
                    c.LCID.Should().Be( 1 );
                    c.Name.Should().Be( "ar" );
                    c.EnglishName.Should().Be( "Arabic" );
                    c.NativeName.Should().Be( "العربية" );
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
                c.XLCID.Should().Be( 10 );
                c.PrimaryCulture.Should().NotBeNull();
                c.PrimaryCulture.Should().BeSameAs( c.Fallbacks[0] );

                c.Fallbacks[0].LCID.Should().Be( 10 );
                c.Fallbacks[0].Name.Should().Be( "es" );
                c.Fallbacks[0].EnglishName.Should().Be( "Spanish" );
                c.Fallbacks[0].NativeName.Should().Be( "español" );

                c.Fallbacks[1].LCID.Should().Be( 9 );
                c.Fallbacks[1].Name.Should().Be( "en" );
                c.Fallbacks[1].EnglishName.Should().Be( "English" );
                c.Fallbacks[1].NativeName.Should().Be( "English" );

                c.Fallbacks[2].LCID.Should().Be( 12 );
                c.Fallbacks[2].Name.Should().Be( "fr" );
                c.Fallbacks[2].EnglishName.Should().Be( "French" );
                c.Fallbacks[2].NativeName.Should().Be( "Français" );

                c.Fallbacks[3].LCID.Should().Be( 22538 );
                c.Fallbacks[3].Name.Should().Be( "es-419" );
                c.Fallbacks[3].EnglishName.Should().Be( "Spanish (Latin America)" );
                c.Fallbacks[3].NativeName.Should().Be( "español (Latinoamérica)" );

                c.Fallbacks[4].LCID.Should().Be( 11274 );
                c.Fallbacks[4].Name.Should().Be( "es-AR" );
                c.Fallbacks[4].EnglishName.Should().Be( "Spanish (Argentina)" );
                c.Fallbacks[4].NativeName.Should().Be( "español (Argentina)" );

                c.Fallbacks[5].LCID.Should().Be( 2060 );
                c.Fallbacks[5].Name.Should().Be( "fr-BE" );
                c.Fallbacks[5].EnglishName.Should().Be( "French (Belgium)" );
                c.Fallbacks[5].NativeName.Should().Be( "français (Belgique)" );

                c.Fallbacks[6].LCID.Should().Be( 4096 );
                c.Fallbacks[6].Name.Should().Be( "fr-BF" );
                c.Fallbacks[6].EnglishName.Should().Be( "French (Burkina Faso)" );
                c.Fallbacks[6].NativeName.Should().Be( "français (Burkina Faso)" );

                c.Fallbacks[7].LCID.Should().Be( 4097 );
                c.Fallbacks[7].Name.Should().Be( "fr-BI" );
                c.Fallbacks[7].EnglishName.Should().Be( "French (Burundi)" );
                c.Fallbacks[7].NativeName.Should().Be( "français (Burundi)" );

                c.Fallbacks[8].LCID.Should().Be( 1 );
                c.Fallbacks[8].Name.Should().Be( "ar" );
                c.Fallbacks[8].EnglishName.Should().Be( "Arabic" );
                c.Fallbacks[8].NativeName.Should().Be( "العربية" );

                c.Fallbacks[9].LCID.Should().Be( 14337 );
                c.Fallbacks[9].Name.Should().Be( "ar-AE" );
                c.Fallbacks[9].EnglishName.Should().Be( "Arabic (United Arab Emirates)" );
                c.Fallbacks[9].NativeName.Should().Be( "العربية الإمارات العربية المتحدة" );

                c.Fallbacks[10].LCID.Should().Be( 15361 );
                c.Fallbacks[10].Name.Should().Be( "ar-BH" );
                c.Fallbacks[10].EnglishName.Should().Be( "Arabic (Bahrain)" );
                c.Fallbacks[10].NativeName.Should().Be( "العربية البحرين" );

                c.Fallbacks.Count.Should().Be( 11 );
            }
        }

        [Test]
        public void reading_CultureDate_or_ExtendedCultureData_with_unexisting_identifier_returns_null()
        {
            var p = SharedEngine.Map.StObjs.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                p.GetCulture( ctx, 3712 ).Should().BeNull();
                p.GetExtendedCulture( ctx, 3712 ).Should().BeNull();
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
                    .Should().BeEquivalentTo( new[] { 10, 9, 12, 1 }, o => o.WithStrictOrdering() );
                p.SetLCIDFallbaks( ctx, 10, new[] { 10, 12 } );
                p.GetExtendedCulture( ctx, 10 ).Fallbacks.Select( c => c.LCID )
                    .Should().BeEquivalentTo( new[] { 10, 12, 9, 1 }, o => o.WithStrictOrdering() );
                p.SetLCIDFallbaks( ctx, 10, new[] { 10, 1 } );
                p.GetExtendedCulture( ctx, 10 ).Fallbacks.Select( c => c.LCID )
                    .Should().BeEquivalentTo( new[] { 10, 1, 12, 9 }, o => o.WithStrictOrdering() );
                p.SetLCIDFallbaks( ctx, 10, new[] { 10, 9, 1, 12 } );
                p.GetExtendedCulture( ctx, 10 ).Fallbacks.Select( c => c.LCID )
                    .Should().BeEquivalentTo( new[] { 10, 9, 1, 12 }, o => o.WithStrictOrdering() );
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
                    .Should().BeEquivalentTo( new[] { 10, 9, 12, 1 }, o => o.WithStrictOrdering() );

                int xlcidSame = p.AssumeXLCID( ctx, new[] { 10, 9, 12, 1 }, allowLCIDMapping: true );
                xlcidSame.Should().Be( 10, "The primary LCID is okay" );

                int xlcid0 = p.AssumeXLCID( ctx, new[] { 10, 9, 12, 1 }, allowLCIDMapping: false );
                xlcid0.Should().NotBe( 10 ).And.BeGreaterThan( 0x10000, "A XLCID has been created." );

                int xlcid1 = p.AssumeXLCID( ctx, new[] { 10, 1, 9, 12 } );
                int xlcid2 = p.AssumeXLCID( ctx, new[] { 10, 12, 1, 9 } );
                xlcid1.Should().NotBe( xlcid2 );
                p.GetExtendedCulture( ctx, xlcid1 ).Fallbacks.Select( c => c.LCID )
                    .Should().BeEquivalentTo( new[] { 10, 1, 9, 12 }, o => o.WithStrictOrdering() );
                p.GetExtendedCulture( ctx, xlcid2 ).Fallbacks.Select( c => c.LCID )
                    .Should().BeEquivalentTo( new[] { 10, 12, 1, 9 }, o => o.WithStrictOrdering() );
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
                    .Should().BeEquivalentTo( new[] { 12, 9, 10, 1 }, o => o.WithStrictOrdering() );

                int xlcid12 = p.AssumeXLCID( ctx, new[] { 12, 1, 9, 10 } );
                p.GetExtendedCulture( ctx, xlcid12 ).Fallbacks.Select( c => c.LCID )
                    .Should().BeEquivalentTo( new[] { 12, 1, 9, 10 }, o => o.WithStrictOrdering() );

                p.DestroyCulture( ctx, 1 );
                p.GetExtendedCulture( ctx, xlcid12 ).Fallbacks.Select( c => c.LCID )
                    .Should().BeEquivalentTo( new[] { 12, 9, 10 }, o => o.WithStrictOrdering() );
                p.GetExtendedCulture( ctx, 12 ).Fallbacks.Select( c => c.LCID )
                    .Should().BeEquivalentTo( new[] { 12, 9, 10 }, o => o.WithStrictOrdering() );
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
                int xlcid1a = p.AssumeXLCID( ctx, new[] { 1, 9, 10, 12 } );
                int xlcid1b = p.AssumeXLCID( ctx, new[] { 1, 10, 9, 12 } );
                p.GetExtendedCulture( ctx, xlcid1a ).Should().NotBeNull();
                p.GetExtendedCulture( ctx, xlcid1b ).Should().NotBeNull();

                p.DestroyCulture( ctx, 1 );
                p.GetExtendedCulture( ctx, xlcid1a ).Should().BeNull();
                p.GetExtendedCulture( ctx, xlcid1b ).Should().BeNull();
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
                int xlcid = p.AssumeXLCID( ctx, new[] { 1, 9, 10, 12 } );
                p.DestroyCulture( ctx, xlcid );
                int xlcid2 = p.AssumeXLCID( ctx, new[] { 1, 9, 10, 12 } );
                xlcid2.Should().BeGreaterThan( xlcid );
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

}
