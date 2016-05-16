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

            Task( "Check-Repository" )
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
               .Does( () =>
               {
                   var testDlls = Cake.ParseSolution( "CK-DB.sln" )
                                        .Projects
                                            .Where( p => p.Name.EndsWith( ".Tests" ) 
                                                            && p.Name != "CK.DB.Acl.AclType.Tests" )
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
                        TransformText( nuspec, configuration, gitInfo );
                        Cake.NuGetPack( nuspec, settings );
                    }
                    Cake.DeleteFiles( releasesDir.Path + "/*.nuspec" );
                } );

            Task( "Push-NuGet-Packages" )
                .IsDependentOn( "Create-NuGet-Packages" )
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
                        if( gitInfo.PreReleaseName == "" || gitInfo.PreReleaseName == "rc" )
                        {
                            PushNuGetPackages( "NUGET_API_KEY", "https://www.nuget.org/api/v2/package", nugetPackages );
                        }
                        else
                        {
                            // An alpha, beta, delta, epsilon, gamma, kappa, prerelease goes to invenietis-prerelease.
                            PushNuGetPackages( "MYGET_PRERELEASE_API_KEY", "https://www.myget.org/F/invenietis-prerelease/api/v2/package", nugetPackages );
                        }
                    }
                    else
                    {
                        Debug.Assert( gitInfo.IsValidCIBuild );
                        PushNuGetPackages( "MYGET_EXPLORE_API_KEY", "https://www.myget.org/F/invenietis-explore/api/v2/package", nugetPackages );
                    }
                } );

            Task( "Default" ).IsDependentOn( "Push-NuGet-Packages" );
        }

        private void TransformText( FilePath textFilePath, string configuration, SimpleRepositoryInfo gitInfo )
        {
            Cake.TransformTextFile( textFilePath, "{{", "}}" )
                    .WithToken( "configuration", configuration )
                    .WithToken( "NuGetVersion", gitInfo.NuGetVersion )
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
