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

            Task( "Restore-NuGet-Packages" )
                .IsDependentOn( "Check-Repository" )
                .Does( () =>
                {
                    Cake.DotNetCoreRestore( coreBuildFile,
                        new DotNetCoreRestoreSettings().AddVersionArguments( gitInfo, c =>
                         {
                            // No impact see: https://github.com/NuGet/Home/issues/3772
                            // c.Verbosity = DotNetCoreRestoreVerbosity.Minimal;
                        } ) );
                } );


            Task( "Build" )
                .IsDependentOn( "Clean" )
                .IsDependentOn( "Restore-NuGet-Packages" )
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
                    settings.ArgumentCustomization = args => args.Append( "--include-symbols" );
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

                  Cake.DotNetCoreRestore( integrationSolution, new DotNetCoreRestoreSettings()
                  {
                      NoCache = true,
                      ArgumentCustomization = c => c.Append( $@"/p:CKDBVersion=""{version}""" )
                  } );

                  Cake.DotNetCoreBuild( integrationSolution, new DotNetCoreBuildSettings()
                  {
                      ArgumentCustomization = c => c.Append( $@"/p:CKDBVersion=""{version}""" )
                  } );
              } );

            Task( "Run-CKDBSetup-On-IntegrationTests-AllPackages" )
              .IsDependentOn( "Compile-IntegrationTests" )
              .Does( () =>
               {
                   var vCKDatabase = XDocument.Load( "Common/DependencyVersions.props" )
                     .Root
                     .Elements( msBuild + "PropertyGroup" )
                     .Elements( msBuild + "CKDatabaseVersion" )
                     .Single()
                     .Value;

                   var exe = System.IO.Path.Combine( Cake.EnvironmentVariable( "UserProfile" ), ".nuget", "packages", "ckdbsetup", vCKDatabase, "tools", "CKDBSetup.exe" );
                   if( !System.IO.File.Exists( exe ) )
                   {
                       Cake.NuGetInstall( "CKDBSetup", new NuGetInstallSettings()
                       {
                           Prerelease = true,
                           Version = vCKDatabase,
                           OutputDirectory = "packages"
                       } );
                   }
                   if( !System.IO.File.Exists( exe ) )
                   {
                       throw new Exception( "Unable to install CKDBSetup " + vCKDatabase );
                   }

                   var projectPath = integrationProjects.Single( p => p.Name == "AllPackages" ).Path.GetDirectory();
                   var binPath = projectPath.Combine( $"bin/{configuration}/net461" );

                   string c = Environment.GetEnvironmentVariable( "CK_DB_TEST_MASTER_CONNECTION_STRING" );
                   if( c == null ) c = System.Configuration.ConfigurationManager.AppSettings["CK_DB_TEST_MASTER_CONNECTION_STRING"];
                   if( c == null ) c = "Server=.;Database=master;Integrated Security=SSPI";
                   var csB = new SqlConnectionStringBuilder( c );
                   csB.InitialCatalog = "TEST_CK_DB_AllPackages";
                   var dbCon = csB.ToString();

                   var cmdLine = $@"{exe} setup ""{dbCon}"" -ra ""AllPackages"" -n ""GenByCKDBSetup"" -p ""{binPath}""";

                   {
                       int result = Cake.RunCmd( cmdLine );
                       if( result != 0 ) throw new Exception( "CKDBSetup.exe failed for IL generation." );
                   }
                   //{
                   //    int result = Cake.RunCmd( cmdLine + " -sg" );
                   //    if( result != 0 ) throw new Exception( "CKDBSetup.exe failed for Source Code generation." );
                   //}
               } );

            Task( "Run-IntegrationTests" )
              .IsDependentOn( "Compile-IntegrationTests" )
              .WithCriteria( () => !Cake.IsInteractiveMode()
                                   || Cake.ReadInteractiveOption( "Run integration tests?", 'Y', 'N' ) == 'Y' )
              .Does( () =>
              {
                  var integrationTests = integrationProjects.Where( p => p.Name.EndsWith( ".Tests" ) );


                  var testDlls = integrationTests
                                  .Select( p => System.IO.Path.Combine(
                                                      p.Path.GetDirectory().ToString(), "bin", configuration, "net461", p.Name + ".dll" ) );
                  Cake.Information( "Testing: {0}", string.Join( ", ", testDlls ) );
                  Cake.NUnit( testDlls, new NUnitSettings() { Framework = "v4.5" } );
              } );

            Task( "Push-NuGet-Packages" )
                    .IsDependentOn( "Create-NuGet-Packages" )
                    .IsDependentOn( "Run-CKDBSetup-On-IntegrationTests-AllPackages" )
                    .IsDependentOn( "Run-IntegrationTests" )
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
                                PushNuGetPackages( "NUGET_API_KEY", "https://www.nuget.org/api/v2/package", nugetPackages );
                            }
                            else
                            {
                                // An alpha, beta, delta, epsilon, gamma, kappa goes to invenietis-preview.
                                PushNuGetPackages( "MYGET_PREVIEW_API_KEY", "https://www.myget.org/F/invenietis-preview/api/v2/package", nugetPackages );
                            }
                        }
                        else
                        {
                            Debug.Assert( gitInfo.IsValidCIBuild );
                            PushNuGetPackages( "MYGET_CI_API_KEY", "https://www.myget.org/F/invenietis-ci/api/v2/package", nugetPackages );
                        }
                        if( Cake.AppVeyor().IsRunningOnAppVeyor )
                        {
                            Cake.AppVeyor().UpdateBuildVersion( gitInfo.SafeNuGetVersion );
                        }
                    } );

            Task( "Default" ).IsDependentOn( "Push-NuGet-Packages" );
        }

        private void PushNuGetPackages( string apiKeyName, string pushUrl, IEnumerable<FilePath> nugetPackages )
        {
            // Resolves the API key.
            var apiKey = Cake.InteractiveEnvironmentVariable( apiKeyName );
            if( string.IsNullOrEmpty( apiKey ) )
            {
                Cake.Information( "Could not resolve {0}. Push to {1} is skipped.", apiKeyName, pushUrl );
            }
            else
            {
                var settings = new NuGetPushSettings
                {
                    Source = pushUrl,
                    ApiKey = apiKey
                };

                foreach( var nupkg in nugetPackages )
                {
                    Cake.NuGetPush( nupkg, settings );
                }
            }
        }

    }
}
