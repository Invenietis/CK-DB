using CKSetup;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using static CK.Testing.DBSetupTestHelper;

namespace AllPackages.Tests
{
    [TestFixture]
    public class MultiBinPathsTests
    {
        [TestCase( "net461" )]
        [TestCase( "netcoreapp2.0" )]
        public void CKSetup_facade_apps_in_Net461( string framework )
        {
            var dirConfigFile = TestHelper.TestProjectFolder.Combine( "../MultiBinPaths" );
            var configFile = dirConfigFile.AppendPart( "CKSetup.template.xml" );
            var confText = System.IO.File.ReadAllText( configFile )
                            .Replace( "{ConnectionString}", TestHelper.GetConnectionString( "CKDB_TEST_MultiBinPaths" ) )
                            .Replace( "{BuildConfiguration}", TestHelper.BuildConfiguration )
                            .Replace( "{TargetFramework}", framework );
            var conf = new SetupConfiguration( XDocument.Parse( confText ) );
            conf.BasePath = dirConfigFile;
            TestHelper.CKSetup.Run( conf ).Should().Match( status => (CKSetupRunResult)status == CKSetupRunResult.Succeed || (CKSetupRunResult)status == CKSetupRunResult.UpToDate );
        }

    }
}
