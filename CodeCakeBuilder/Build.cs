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
        public Build()
        {
            var releasesDir = Cake.Directory( "CodeCakeBuilder/Releases" );

            string configuration = null;
            SimpleRepositoryInfo gitInfo = null;
            var solution = Cake.ParseSolution( "CK-DB.sln" );

            string CKDatabaseVersion = null;

            Task( "Check-Dependencies" )
            .Does( () =>
            {
                var allPackages = solution.Projects
                                    .Where( p => p.Name != "CodeCakeBuilder" )
                                    .Select( p => new
                                    {
                                        Project = p,
                                        PackageConfig = p.Path.GetDirectory().CombineWithFilePath( "packages.config" ).FullPath
                                    } )
                                    .Where( p => System.IO.File.Exists( p.PackageConfig ) )
                                    .SelectMany( p => XDocument.Load( p.PackageConfig )
                                                    .Root
                                                    .Elements( "package" )
                                                    .Select( e => { e.AddAnnotation( p.Project ); return e; } ) )
                                    .ToList();
                var byPackage = allPackages
                                    .GroupBy( e => e.Attribute( "id" ).Value,
                                              e => new
                                              {
                                                  ProjectName = e.Annotation<SolutionProject>().Name,
                                                  Version = e.Attribute( "version" ).Value
                                              } );
                CKDatabaseVersion = byPackage.Single( e => e.Key == "CK.StObj.Model" ).First().Version;
                var multiVersions = byPackage.Where( g => g.GroupBy( x => x.Version ).Count() > 1 );
                if( multiVersions.Any() )
                {
                    var conflicts = multiVersions.Select( e => Environment.NewLine + " - " + e.Key + ":" + Environment.NewLine + "    - " + string.Join( Environment.NewLine + "    - ", e.GroupBy( x => x.Version ).Select( x => x.Key + " in " + string.Join(", ", x.Select( xN => xN.ProjectName ) ) ) ) );
                    Cake.TerminateWithError( $"Dependency versions differ for:{Environment.NewLine}{string.Join( Environment.NewLine, conflicts )}" );
                }
            } );

            Task( "Check-Repository" )
                .IsDependentOn( "Check-Dependencies" )
                .Does( () =>
                {
                    gitInfo = Cake.GetSimpleRepositoryInfo();
                    if( !gitInfo.IsValid )
                    {
                        if( Cake.IsInteractiveMode()
                            && Cake.ReadInteractiveOption( "Repository is not ready to be published. Proceed anyway?", 'Y', 'N' ) == 'Y' )
                        {
                            Cake.Warning( "GitInfo is not valid, but you choose to continue..." );
                        }
                        else throw new Exception( "Repository is not ready to be published." );
                    }
                    configuration = gitInfo.IsValidRelease && gitInfo.PreReleaseName.Length == 0 ? "Release" : "Debug";
                    Cake.Information( "Publishing {0} in {1}.", gitInfo.SemVer, configuration );
                } );

            Task( "Clean" )
                .IsDependentOn( "Check-Repository" )
                .Does( () =>
                {
                    Cake.CleanDirectories( "**/bin/" + configuration, d => !d.Path.Segments.Contains( "CodeCakeBuilder" ) );
                    Cake.CleanDirectories( "**/obj/" + configuration, d => !d.Path.Segments.Contains( "CodeCakeBuilder" ) );
                    Cake.CleanDirectories( releasesDir );
                } );

            Task( "Restore-NuGet-Packages" )
                .Does( () =>
                {
                    Cake.NuGetRestore( "CK-DB.sln" );
                } );


            Task( "Build" )
                .IsDependentOn( "Clean" )
                .IsDependentOn( "Restore-NuGet-Packages" )
                .IsDependentOn( "Check-Repository" )
                .Does( () =>
                {
                    using( var tempSln = Cake.CreateTemporarySolutionFile( "CK-DB.sln" ) )
                    {
                        tempSln.ExcludeProjectsFromBuild( "CodeCakeBuilder" );
                        Cake.MSBuild( tempSln.FullPath, settings =>
                        {
                            settings.Configuration = configuration;
                            settings.Verbosity = Verbosity.Minimal;
                            // Always generates Xml documentation. Relies on this definition in the csproj files:
                            //
                            // <PropertyGroup Condition=" $(GenerateDocumentation) != '' ">
                            //   <DocumentationFile>bin\$(Configuration)\$(AssemblyName).xml</DocumentationFile>
                            // </PropertyGroup>
                            //
                            settings.Properties.Add( "GenerateDocumentation", new[] { "true" } );
                        } );
                    }
                } );

            Task( "Unit-Testing" )
               .IsDependentOn( "Build" )
                .WithCriteria( () => gitInfo.IsValidRelease )
               .Does( () =>
               {
                   var testDlls = solution.Projects
                                            .Where( p => p.Name.EndsWith( ".Tests" ) )
                                            .Select( p => p.Path.GetDirectory().CombineWithFilePath( "bin/" + configuration + "/" + p.Name + ".dll" ) );
                   Cake.Information( "Testing: {0}", string.Join( ", ", testDlls.Select( p => p.GetFilename().ToString() ) ) );
                   Cake.NUnit( testDlls, new NUnitSettings() { Framework = "v4.5" } );
               } );

            Task( "Create-NuGet-Packages" )
                .IsDependentOn( "Unit-Testing" )
                .Does( () =>
                {
                    Cake.CreateDirectory( releasesDir );
                    var settings = new NuGetPackSettings()
                    {
                        Version = gitInfo.NuGetVersion,
                        BasePath = Cake.Environment.WorkingDirectory,
                        OutputDirectory = releasesDir
                    };
                    Cake.CopyFiles( "CodeCakeBuilder/NuSpec/*.nuspec", releasesDir );
                    foreach( var nuspec in Cake.GetFiles( releasesDir.Path + "/*.nuspec" ) )
                    {
                        TransformText( nuspec, configuration, gitInfo, CKDatabaseVersion );
                        Cake.NuGetPack( nuspec, settings );
                    }
                    Cake.DeleteFiles( releasesDir.Path + "/*.nuspec" );
                } );

            Task( "Run-IntegrationTests" )
                    .IsDependentOn( "Create-NuGet-Packages" )
                    .WithCriteria( () => gitInfo.IsValid )
                    .WithCriteria( () => !Cake.IsInteractiveMode() 
                                            || Cake.ReadInteractiveOption( "Run integration tests?", 'Y', 'N' ) == 'Y' )
                    .Does( () =>
                    {
                        var integrationSolution = "IntegrationTests/IntegrationTests.sln";
                        var integration = Cake.ParseSolution( integrationSolution );
                        var packageConfigFiles = integration.Projects
                                                    .Where( p => p.Name != "CodeCakeBuilder" )
                                                    .Select( p => p.Path.GetDirectory().CombineWithFilePath( "packages.config" ).FullPath )
                                                    .Where( p => System.IO.File.Exists( p ) );
                        var projectNames = new HashSet<string>( solution.Projects.Select( pub => pub.Name ) );
                        foreach( var config in packageConfigFiles )
                        {
                            XDocument doc = XDocument.Load( config );
                            int countRef = 0;
                            foreach( var p in doc.Root.Elements( "package" ) )
                            {
                                if( projectNames.Contains( p.Attribute("id").Value ) 
                                    && p.Attribute( "version" ).Value != gitInfo.NuGetVersion )
                                {
                                    p.SetAttributeValue( "version", gitInfo.NuGetVersion );
                                    ++countRef;
                                }
                            }
                            if( countRef > 0 )
                            {
                                Cake.Information( $"Updated {countRef} in file {config}." );
                                doc.Save( config );
                            }
                        }
                        Cake.NuGetRestore( integrationSolution );
                        Cake.MSBuild( "IntegrationTests/CodeCakeBuilder/CodeCakeBuilder.csproj", settings =>
                        {
                            settings.Configuration = configuration;
                            settings.Verbosity = Verbosity.Minimal;
                        } );
                        Cake.StartProcess( $"IntegrationTests/CodeCakeBuilder/bin/{configuration}/CodeCakeBuilder.exe", "-" + InteractiveAliases.NoInteractionArgument );
                    } );

            Task( "Push-NuGet-Packages" )
                    .IsDependentOn( "Create-NuGet-Packages" )
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
                    } );

            //Task( "Default" ).IsDependentOn( "Run-IntegrationTests" );
            Task( "Default" ).IsDependentOn( "Push-NuGet-Packages" );
        }

        private void TransformText( FilePath textFilePath, string configuration, SimpleRepositoryInfo gitInfo, string ckDatabaseVersion )
        {
            Cake.TransformTextFile( textFilePath, "{{", "}}" )
                    .WithToken( "configuration", configuration )
                    .WithToken( "NuGetVersion", gitInfo.NuGetVersion )
                    .WithToken( "CKDatabaseVersion", ckDatabaseVersion )
                    .WithToken( "CSemVer", gitInfo.SemVer )
                    .Save( textFilePath );
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
