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
    public class ExtendedCultureTests
    {
        [Test]
        public void checking_Idx_updates_when_destroying_a_Culture()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
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
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                RegisterSampleCultures( p, ctx );
                {
                    var c = p.GetCulture( ctx, 10 );
                    Assert.That( c.LCID, Is.EqualTo( 10 ) );
                    Assert.That( c.Name, Is.EqualTo( "es" ) );
                    Assert.That( c.EnglishName, Is.EqualTo( "Spanish" ) );
                    Assert.That( c.NativeName, Is.EqualTo( "español" ) );
                }
                {
                    var c = p.GetCulture( ctx, 1 );
                    Assert.That( c.LCID, Is.EqualTo( 1 ) );
                    Assert.That( c.Name, Is.EqualTo( "ar" ) );
                    Assert.That( c.EnglishName, Is.EqualTo( "Arabic" ) );
                    Assert.That( c.NativeName, Is.EqualTo( "العربية" ) );
                }
            }
        }

        [Test]
        public void reading_ExtendedCultureData()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                CultureTests.RestoreDatabaseToEnglishAndFrenchOnly( p );
                RegisterSampleCultures( p, ctx );
                var c = p.GetExtendedCulture( ctx, 10 );
                Assert.That( c.XLCID, Is.EqualTo( 10 ) );
                Assert.That( c.PrimaryCulture, Is.Not.Null );
                Assert.That( c.PrimaryCulture, Is.SameAs( c.Fallbacks[0] ) );

                Assert.That( c.Fallbacks[0].LCID, Is.EqualTo( 10 ) );
                Assert.That( c.Fallbacks[0].Name, Is.EqualTo( "es" ) );
                Assert.That( c.Fallbacks[0].EnglishName, Is.EqualTo( "Spanish" ) );
                Assert.That( c.Fallbacks[0].NativeName, Is.EqualTo( "español" ) );

                Assert.That( c.Fallbacks[1].LCID, Is.EqualTo( 9 ) );
                Assert.That( c.Fallbacks[1].Name, Is.EqualTo( "en" ) );
                Assert.That( c.Fallbacks[1].EnglishName, Is.EqualTo( "English" ) );
                Assert.That( c.Fallbacks[1].NativeName, Is.EqualTo( "English" ) );

                Assert.That( c.Fallbacks[2].LCID, Is.EqualTo( 12 ) );
                Assert.That( c.Fallbacks[2].Name, Is.EqualTo( "fr" ) );
                Assert.That( c.Fallbacks[2].EnglishName, Is.EqualTo( "French" ) );
                Assert.That( c.Fallbacks[2].NativeName, Is.EqualTo( "Français" ) );

                Assert.That( c.Fallbacks[3].LCID, Is.EqualTo( 22538 ) );
                Assert.That( c.Fallbacks[3].Name, Is.EqualTo( "es-419" ) );
                Assert.That( c.Fallbacks[3].EnglishName, Is.EqualTo( "Spanish (Latin America)" ) );
                Assert.That( c.Fallbacks[3].NativeName, Is.EqualTo( "español (Latinoamérica)" ) );

                Assert.That( c.Fallbacks[4].LCID, Is.EqualTo( 11274 ) );
                Assert.That( c.Fallbacks[4].Name, Is.EqualTo( "es-AR" ) );
                Assert.That( c.Fallbacks[4].EnglishName, Is.EqualTo( "Spanish (Argentina)" ) );
                Assert.That( c.Fallbacks[4].NativeName, Is.EqualTo( "español (Argentina)" ) );

                Assert.That( c.Fallbacks[5].LCID, Is.EqualTo( 2060 ) );
                Assert.That( c.Fallbacks[5].Name, Is.EqualTo( "fr-BE" ) );
                Assert.That( c.Fallbacks[5].EnglishName, Is.EqualTo( "French (Belgium)" ) );
                Assert.That( c.Fallbacks[5].NativeName, Is.EqualTo( "français (Belgique)" ) );

                Assert.That( c.Fallbacks[6].LCID, Is.EqualTo( 4096 ) );
                Assert.That( c.Fallbacks[6].Name, Is.EqualTo( "fr-BF" ) );
                Assert.That( c.Fallbacks[6].EnglishName, Is.EqualTo( "French (Burkina Faso)" ) );
                Assert.That( c.Fallbacks[6].NativeName, Is.EqualTo( "français (Burkina Faso)" ) );

                Assert.That( c.Fallbacks[7].LCID, Is.EqualTo( 4097 ) );
                Assert.That( c.Fallbacks[7].Name, Is.EqualTo( "fr-BI" ) );
                Assert.That( c.Fallbacks[7].EnglishName, Is.EqualTo( "French (Burundi)" ) );
                Assert.That( c.Fallbacks[7].NativeName, Is.EqualTo( "français (Burundi)" ) );

                Assert.That( c.Fallbacks[8].LCID, Is.EqualTo( 1 ) );
                Assert.That( c.Fallbacks[8].Name, Is.EqualTo( "ar" ) );
                Assert.That( c.Fallbacks[8].EnglishName, Is.EqualTo( "Arabic" ) );
                Assert.That( c.Fallbacks[8].NativeName, Is.EqualTo( "العربية" ) );

                Assert.That( c.Fallbacks[9].LCID, Is.EqualTo( 14337 ) );
                Assert.That( c.Fallbacks[9].Name, Is.EqualTo( "ar-AE" ) );
                Assert.That( c.Fallbacks[9].EnglishName, Is.EqualTo( "Arabic (United Arab Emirates)" ) );
                Assert.That( c.Fallbacks[9].NativeName, Is.EqualTo( "العربية الإمارات العربية المتحدة" ) );

                Assert.That( c.Fallbacks[10].LCID, Is.EqualTo( 15361 ) );
                Assert.That( c.Fallbacks[10].Name, Is.EqualTo( "ar-BH" ) );
                Assert.That( c.Fallbacks[10].EnglishName, Is.EqualTo( "Arabic (Bahrain)" ) );
                Assert.That( c.Fallbacks[10].NativeName, Is.EqualTo( "العربية البحرين" ) );

                Assert.That( c.Fallbacks.Count, Is.EqualTo( 11 ) );
            }
        }

        [Test]
        public void reading_CultureDate_or_ExtendedCultureData_with_unexisting_identifier_returns_null()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                Assert.That( p.GetCulture( ctx, 3712 ), Is.Null );
                Assert.That( p.GetExtendedCulture( ctx, 3712 ), Is.Null );
            }
        }

        [Test]
        public void setting_culture_fallbacks()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                CultureTests.RestoreDatabaseToEnglishAndFrenchOnly( p );
                RegisterSpanish( p, ctx );
                RegisterArabic( p, ctx );
                CollectionAssert.AreEqual( p.GetExtendedCulture( ctx, 10 ).Fallbacks.Select( c => c.LCID ), new[] { 10, 9, 12, 1 } );
                p.SetLCIDFallbaks( ctx, 10, new[] { 10, 12 } );
                CollectionAssert.AreEqual( p.GetExtendedCulture( ctx, 10 ).Fallbacks.Select( c => c.LCID ), new[] { 10, 12, 9, 1 } );
                p.SetLCIDFallbaks( ctx, 10, new[] { 10, 1 } );
                CollectionAssert.AreEqual( p.GetExtendedCulture( ctx, 10 ).Fallbacks.Select( c => c.LCID ), new[] { 10, 1, 12, 9 } );
                p.SetLCIDFallbaks( ctx, 10, new[] { 10, 9, 1, 12 } );
                CollectionAssert.AreEqual( p.GetExtendedCulture( ctx, 10 ).Fallbacks.Select( c => c.LCID ), new[] { 10, 9, 1, 12 } );
            }
        }

        [Test]
        public void assuming_extended_cultures()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                CultureTests.RestoreDatabaseToEnglishAndFrenchOnly( p );
                RegisterSpanish( p, ctx );
                RegisterArabic( p, ctx );
                CollectionAssert.AreEqual( p.GetExtendedCulture( ctx, 10 ).Fallbacks.Select( c => c.LCID ), new[] { 10, 9, 12, 1 } );

                int xlcidSame = p.AssumeXLCID( ctx, new[] { 10, 9, 12, 1 }, allowLCIDMapping: true );
                Assert.That( xlcidSame, Is.EqualTo( 10 ), "The primary LCID is okay" );

                int xlcid0 = p.AssumeXLCID( ctx, new[] { 10, 9, 12, 1 }, allowLCIDMapping: false );
                Assert.That( xlcid0, Is.Not.EqualTo( 10 ).And.GreaterThan( 0x10000 ), "A XLCID has been created." );

                int xlcid1 = p.AssumeXLCID( ctx, new[] { 10, 1, 9, 12 } );
                int xlcid2 = p.AssumeXLCID( ctx, new[] { 10, 12, 1, 9 } );
                Assert.That( xlcid1, Is.Not.EqualTo( xlcid2 ) );
                CollectionAssert.AreEqual( p.GetExtendedCulture( ctx, xlcid1 ).Fallbacks.Select( c => c.LCID ), new[] { 10, 1, 9, 12 } );
                CollectionAssert.AreEqual( p.GetExtendedCulture( ctx, xlcid2 ).Fallbacks.Select( c => c.LCID ), new[] { 10, 12, 1, 9 } );
            }
        }

        [Test]
        public void destroying_culture_updates_all_cultures_fallbacks()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                CultureTests.RestoreDatabaseToEnglishAndFrenchOnly( p );
                RegisterSpanish( p, ctx );
                RegisterArabic( p, ctx );
                CollectionAssert.AreEqual( p.GetExtendedCulture( ctx, 12 ).Fallbacks.Select( c => c.LCID ), new[] { 12, 9, 10, 1 } );

                int xlcid12 = p.AssumeXLCID( ctx, new[] { 12, 1, 9, 10 } );
                CollectionAssert.AreEqual( p.GetExtendedCulture( ctx, xlcid12 ).Fallbacks.Select( c => c.LCID ), new[] { 12, 1, 9, 10 } );

                p.DestroyCulture( ctx, 1 );
                CollectionAssert.AreEqual( p.GetExtendedCulture( ctx, xlcid12 ).Fallbacks.Select( c => c.LCID ), new[] { 12, 9, 10 } );
                CollectionAssert.AreEqual( p.GetExtendedCulture( ctx, 12 ).Fallbacks.Select( c => c.LCID ), new[] { 12, 9, 10 } );
            }
        }

        [Test]
        public void destroying_LCID_destroys_XLCID_with_the_primary_LCID()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                CultureTests.RestoreDatabaseToEnglishAndFrenchOnly( p );
                RegisterSpanish( p, ctx );
                RegisterArabic( p, ctx );
                int xlcid1a = p.AssumeXLCID( ctx, new[] { 1, 9, 10, 12 } );
                int xlcid1b = p.AssumeXLCID( ctx, new[] { 1, 10, 9, 12 } );
                Assert.That( p.GetExtendedCulture( ctx, xlcid1a ), Is.Not.Null );
                Assert.That( p.GetExtendedCulture( ctx, xlcid1b ), Is.Not.Null );

                p.DestroyCulture( ctx, 1 );
                Assert.That( p.GetExtendedCulture( ctx, xlcid1a ), Is.Null );
                Assert.That( p.GetExtendedCulture( ctx, xlcid1b ), Is.Null );
            }
        }

        [Test]
        public void XLCID_are_never_reused()
        {
            var p = TestHelper.StObjMap.Default.Obtain<Package>();
            using( var ctx = new SqlStandardCallContext() )
            {
                RegisterSpanish( p, ctx );
                RegisterArabic( p, ctx );
                int xlcid = p.AssumeXLCID( ctx, new[] { 1, 9, 10, 12 } );
                p.DestroyCulture( ctx, xlcid );
                int xlcid2 = p.AssumeXLCID( ctx, new[] { 1, 9, 10, 12 } );
                Assert.That( xlcid2, Is.GreaterThan( xlcid ) );
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
