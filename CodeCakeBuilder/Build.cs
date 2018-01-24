using Cake.Common;
using Cake.Common.Solution;
using Cake.Common.IO;
using Cake.Common.Tools.MSBuild;
using Cake.Common.Tools.NuGet;
using Cake.Core;
using Cake.Common.Diagnostics;
using Code.Cake;
using Cake.Common.Tools.NuGet.Pack;
using System.Linq;
using Cake.Core.Diagnostics;
using Cake.Common.Tools.NuGet.Restore;
using System;
using Cake.Common.Tools.NuGet.Push;
using SimpleGitVersion;
using Cake.Common.Tools.NUnit;
using System.Collections.Generic;
using Cake.Common.Text;
using Cake.Core.IO;
using System.Diagnostics;
using System.Xml.Linq;
using System.Reflection;
using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Restore;
using Cake.Common.Tools.DotNetCore.Build;
using Cake.Common.Tools.DotNetCore.Pack;
using Cake.Common.Build;
using System.Data.SqlClient;
using Cake.Common.Tools.NuGet.Install;
using System.Net.Http;
using Cake.Common.Net;
using Cake.Common.Tools.DotNetCore.Publish;

namespace CodeCake
{

    /// <summary>
    /// Sample build "script".
    /// Build scripts can be decorated with AddPath attributes that inject existing paths into the PATH environment variable. 
    /// </summary>
    [AddPath( "CodeCakeBuilder/Tools" )]
    [AddPath( "packages/**/tools*" )]
    public class Build : CodeCakeHost
    {
        XNamespace msBuild = "http://schemas.microsoft.com/developer/msbuild/2003";

        public Build()
        {
            Cake.Log.Verbosity = Verbosity.Diagnostic;

            const string solutionName = "CK-DB";
            const string solutionFileName = solutionName + ".sln";
            const string integrationSolution = "IntegrationTests/IntegrationTests.sln";
            const string integrationTestsDirectory = "IntegrationTests/Tests/AllPackages.Tests";
            const string integrationTestsCSProj = integrationTestsDirectory+ "/AllPackages.Tests.csproj";

            var releasesDir = Cake.Directory( "CodeCakeBuilder/Releases" );
            var coreBuildFile = Cake.File( "CodeCakeBuilder/CoreBuild.proj" );

            var projects = Cake.ParseSolution( solutionFileName )
                                       .Projects
                                       .Where( p => !(p is SolutionFolder)
                                                     && !p.Path.Segments.Contains( "IntegrationTests" )
                                                     && p.Name != "CodeCakeBuilder" );

            var integrationProjects = Cake.ParseSolution( integrationSolution )
                            .Projects
                            .Where( p => !(p is SolutionFolder) );

            // We publish .Tests projects for this solution.
            var projectsToPublish = projects;

            SimpleRepositoryInfo gitInfo = Cake.GetSimpleRepositoryInfo();

            // Configuration is either "Debug" or "Release".
            string configuration = "Debug";

            var vCKDatabase = XDocument.Load( "Common/DependencyVersions.props" )
                                          .Root
                                          .Elements( msBuild + "PropertyGroup" )
                                          .Elements( msBuild + "CKDatabaseVersion" )
                                          .Single()
                                          .Value;
            Cake.Information( $"Using CK-Database version {vCKDatabase}." );
            string ckSetupNet461Path = System.IO.Path.GetFullPath( System.IO.Path.Combine( releasesDir, "CKSetup-Net461" ) );

            Task( "Check-Repository" )
               .Does( () =>
               {
                   if( !gitInfo.IsValid )
                   {
                       if( Cake.IsInteractiveMode()
                           && Cake.ReadInteractiveOption( "Repository is not ready to be published. Proceed anyway?", 'Y', 'N' ) == 'Y' )
                       {
                           Cake.Warning( "GitInfo is not valid, but you choose to continue..." );
                       }
                       else if( !Cake.AppVeyor().IsRunningOnAppVeyor ) throw new Exception( "Repository is not ready to be published." );
                   }
                   Debug.Assert( configuration == "Debug" );
                   if( gitInfo.IsValidRelease
                       && (gitInfo.PreReleaseName.Length == 0 || gitInfo.PreReleaseName == "rc") )
                   {
                       configuration = "Release";
                   }

                   Cake.Information( "Publishing {0} projects with version={1} and configuration={2}: {3}",
                       projectsToPublish.Count(),
                       gitInfo.SafeSemVersion,
                       configuration,
                       string.Join( ", ", projectsToPublish.Select( p => p.Name ) ) );
               } );

            Task( "Clean" )
                .IsDependentOn( "Check-Repository" )
                .Does( () =>
                 {
                     Cake.CleanDirectories( projects.Select( p => p.Path.GetDirectory().Combine( "bin" ) ) );
                     Cake.CleanDirectories( releasesDir );
                     Cake.DeleteFiles( "Tests/**/TestResult*.xml" );
                 } );

            Task( "Build" )
                .IsDependentOn( "Clean" )
                .IsDependentOn( "Check-Repository" )
                .Does( () =>
                {
                    Cake.DotNetCoreBuild( coreBuildFile,
                        new DotNetCoreBuildSettings().AddVersionArguments( gitInfo, s =>
                         {
                             s.Configuration = configuration;
                         } ) );
                } );

            Task( "Unit-Testing" )
                .IsDependentOn( "Build" )
                .WithCriteria( () => !Cake.IsInteractiveMode()
                                        || Cake.ReadInteractiveOption( "Run unit tests?", 'Y', 'N' ) == 'Y' )
               .Does( () =>
               {
                   var testDlls = projects
                                    .Where( p => p.Name.EndsWith( ".Tests" ) )
                                    .Select( p => p.Path.GetDirectory().CombineWithFilePath( "bin/" + configuration + "/net461/" + p.Name + ".dll" ) );
                   Cake.Information( "Testing: {0}", string.Join( ", ", testDlls.Select( p => p.GetFilename().ToString() ) ) );
                   Cake.NUnit( testDlls, new NUnitSettings() { Framework = "v4.5" } );
               } );

            Task( "Create-NuGet-Packages" )
                .IsDependentOn( "Unit-Testing" )
                .Does( () =>
                {
                    Cake.CreateDirectory( releasesDir );
                    var settings = new DotNetCorePackSettings();
                    // Waiting for netcore 2.1 (https://github.com/dotnet/cli/issues/5331).
                    // Setting nobuild and BuildProjectReferences=false
                    settings.ArgumentCustomization = args => args.Append( "--include-symbols" )
                                                                  // Why is it required for Tests package?
                                                                  // Without this Pack on Tests projects does not
                                                                  // generate nupkg.
                                                                 .Append( "/p:IsPackable=true" )
                                                                 .Append( "/p:BuildProjectReferences=false" );
                    settings.NoBuild = true;
                    settings.Configuration = configuration;
                    settings.OutputDirectory = releasesDir;
                    settings.AddVersionArguments( gitInfo );
                    Cake.DotNetCorePack( coreBuildFile, settings );
                } );

            Task( "Compile-IntegrationTests" )
              .IsDependentOn( "Create-NuGet-Packages" )
              .Does( () =>
              {
                  if( !gitInfo.IsValid )
                  {
                      string nugetV3Cache = Environment.ExpandEnvironmentVariables( @"%USERPROFILE%/.nuget/packages" );
                      Cake.CleanDirectories( nugetV3Cache + @"/**/" + CSemVer.SVersion.ZeroVersion );
                  }

                  string version = gitInfo.IsValid
                                    ? gitInfo.SafeNuGetVersion
                                    : CSemVer.SVersion.ZeroVersion.ToString();

                  Cake.DotNetCoreBuild( integrationSolution, new DotNetCoreBuildSettings()
                  {
                      ArgumentCustomization = c => c.Append( $@"/p:CKDBVersion=""{version}""" )
                  } );
                  Cake.DotNetCorePublish( integrationTestsCSProj, new DotNetCorePublishSettings()
                  {
                      ArgumentCustomization = c => c.Append( $@"/p:CKDBVersion=""{version}""" ).Append( " /p:IsPackable=true" ),
                      Framework = "netcoreapp2.0"
                  } );
              } );

            Task( "Download-CKSetup-Net461-From-Store-and-Unzip-it" )
                .Does( () =>
                {
                    var tempFile = Cake.DownloadFile( "http://cksetup.invenietis.net/dl-zip/CKSetup/Net461" );
                    Cake.Unzip( tempFile, ckSetupNet461Path );
                } );

            Task( "Run-CKSetup-On-IntegrationTests-AllPackages-Net461-With-CKSetup-Net461" )
              .IsDependentOn( "Compile-IntegrationTests" )
              .IsDependentOn( "Download-CKSetup-Net461-From-Store-and-Unzip-it" )
              .Does( () =>
              {
                  var binPath = System.IO.Path.GetFullPath( integrationTestsDirectory + $"/bin/{configuration}/net461" );
                  string dbCon = GetConnectionStringForIntegrationTestsAllPackages();

                  string configFile = System.IO.Path.Combine( releasesDir, "CKSetup-IntegrationTests-AllPackages-Net461.xml" );
                  Cake.TransformTextFile( "CodeCakeBuilder/CKSetup-IntegrationTests-AllPackages.xml", "{", "}" )
                        .WithToken( "binPath", binPath )
                        .WithToken( "connectionString", dbCon )
                        .Save( configFile );

                  var cmdLine = $@"{ckSetupNet461Path}\CKSetup.exe run ""{configFile}"" -v Monitor ";
                  {
                      int result = Cake.RunCmd( cmdLine );
                      if( result != 0 ) throw new Exception( "CKSetup.exe failed." );
                  }
              } );

            Task( "Run-IntegrationTests" )
              .IsDependentOn( "Compile-IntegrationTests" )
              .WithCriteria( () => !Cake.IsInteractiveMode()
                                   || Cake.ReadInteractiveOption( "Run integration tests?", 'Y', 'N' ) == 'Y' )
              .Does( () =>
              {
                  // Running AllPackages.Tests executes a CKSetup on MultiBinPaths with the
                  // 3 applications (FacadeApp) on the CKDB_TEST_MultiBinPaths database.
                  // The task "Run-Facade-App-Tests" below executes the 3 tests apps which
                  // use the burned connection string of the generated StObjMap.
                  //
                  var integrationTests = integrationProjects.Where( p => p.Name == "AllPackages.Tests" );

                  var testDlls = integrationTests
                                  .Select( p => System.IO.Path.Combine(
                                                      p.Path.GetDirectory().ToString(), "bin", configuration, "net461", p.Name + ".dll" ) );
                  Cake.Information( $"Testing: {string.Join( ", ", testDlls )}" );
                  Cake.NUnit( testDlls, new NUnitSettings() { Framework = "v4.5" } );
              } );

            Task( "Run-Facade-App-Tests" )
              .IsDependentOn( "Run-IntegrationTests" )
              .WithCriteria( () => !Cake.IsInteractiveMode()
                                   || Cake.ReadInteractiveOption( "Run Facade application tests?", 'Y', 'N' ) == 'Y' )
              .Does( () =>
              {
                  var facadeTests = integrationProjects.Where( p => p.Name.StartsWith( "FacadeApp" ) );

                  var testNet461Dlls = facadeTests
                                  .Select( p => System.IO.Path.Combine(
                                                      p.Path.GetDirectory().ToString(), "bin", configuration, "net461", p.Name + ".dll" ) );
                  Cake.Information( $"Testing: {string.Join( ", ", testNet461Dlls )}" );
                  Cake.NUnit( testNet461Dlls, new NUnitSettings() { Framework = "v4.5" } );
              } );

            Task( "Push-NuGet-Packages" )
                    .IsDependentOn( "Create-NuGet-Packages" )
                    .IsDependentOn( "Run-CKSetup-On-IntegrationTests-AllPackages-Net461-With-CKSetup-Net461" )
                    .IsDependentOn( "Run-IntegrationTests" )
                    .IsDependentOn( "Run-Facade-App-Tests" )
                    .WithCriteria( () => gitInfo.IsValid )
                    .Does( () =>
                    {
                        IEnumerable<FilePath> nugetPackages = Cake.GetFiles( releasesDir.Path + "/*.nupkg" );
                        if( Cake.IsInteractiveMode() )
                        {
                            var localFeed = Cake.FindDirectoryAbove( "LocalFeed" );
                            if( localFeed != null )
                            {
                                Cake.Information( "LocalFeed directory found: {0}", localFeed );
                                if( Cake.ReadInteractiveOption( "Do you want to publish to LocalFeed?", 'Y', 'N' ) == 'Y' )
                                {
                                    Cake.CopyFiles( nugetPackages, localFeed );
                                }
                            }
                        }
                        if( gitInfo.IsValidRelease )
                        {
                            if( gitInfo.PreReleaseName == ""
                                || gitInfo.PreReleaseName == "rc"
                                || gitInfo.PreReleaseName == "prerelease" )
                            {
                                PushNuGetPackages( "MYGET_RELEASE_API_KEY",
                                                    "https://www.myget.org/F/invenietis-release/api/v2/package",
                                                    "https://www.myget.org/F/invenietis-release/symbols/api/v2/package",
                                                    nugetPackages );
                            }
                            else
                            {
                                // An alpha, beta, delta, epsilon, gamma, kappa goes to invenietis-preview.
                                PushNuGetPackages( "MYGET_PREVIEW_API_KEY",
                                                    "https://www.myget.org/F/invenietis-preview/api/v2/package",
                                                    "https://www.myget.org/F/invenietis-preview/symbols/api/v2/package",
                                                    nugetPackages );
                            }
                        }
                        else
                        {
                            Debug.Assert( gitInfo.IsValidCIBuild );
                            PushNuGetPackages( "MYGET_CI_API_KEY",
                                                "https://www.myget.org/F/invenietis-ci/api/v2/package",
                                                "https://www.myget.org/F/invenietis-ci/symbols/api/v2/package",
                                                nugetPackages );
                        }
                        if( Cake.AppVeyor().IsRunningOnAppVeyor )
                        {
                            Cake.AppVeyor().UpdateBuildVersion( gitInfo.SafeNuGetVersion );
                        }
                    } );

            Task( "Default" ).IsDependentOn( "Push-NuGet-Packages" );
        }

        private static string GetConnectionStringForIntegrationTestsAllPackages()
        {
            string c = Environment.GetEnvironmentVariable( "CK_DB_TEST_MASTER_CONNECTION_STRING" );
            if( c == null ) c = System.Configuration.ConfigurationManager.AppSettings["CK_DB_TEST_MASTER_CONNECTION_STRING"];
            if( c == null ) c = "Server=.;Database=master;Integrated Security=SSPI";
            var csB = new SqlConnectionStringBuilder( c );
            csB.InitialCatalog = "TEST_CK_DB_AllPackages";
            var dbCon = csB.ToString();
            return dbCon;
        }

        void PushNuGetPackages( string apiKeyName, string pushUrl, string pushSymbolUrl, IEnumerable<FilePath> nugetPackages )
        {
            // Resolves the API key.
            var apiKey = Cake.InteractiveEnvironmentVariable( apiKeyName );
            if( string.IsNullOrEmpty( apiKey ) )
            {
                Cake.Information( $"Could not resolve {apiKeyName}. Push to {pushUrl} is skipped." );
            }
            else
            {
                var settings = new NuGetPushSettings
                {
                    Source = pushUrl,
                    ApiKey = apiKey,
                    Verbosity = NuGetVerbosity.Detailed
                };
                NuGetPushSettings symbSettings = null;
                if( pushSymbolUrl != null )
                {
                    symbSettings = new NuGetPushSettings
                    {
                        Source = pushSymbolUrl,
                        ApiKey = apiKey,
                        Verbosity = NuGetVerbosity.Detailed
                    };
                }
                foreach( var nupkg in nugetPackages )
                {
                    if( !nupkg.FullPath.EndsWith( ".symbols.nupkg" ) )
                    {
                        Cake.Information( $"Pushing '{nupkg}' to '{pushUrl}'." );
                        Cake.NuGetPush( nupkg, settings );
                    }
                    else
                    {
                        if( symbSettings != null )
                        {
                            Cake.Information( $"Pushing Symbols '{nupkg}' to '{pushSymbolUrl}'." );
                            Cake.NuGetPush( nupkg, symbSettings );
                        }
                    }
                }
            }
        }

    }
}
