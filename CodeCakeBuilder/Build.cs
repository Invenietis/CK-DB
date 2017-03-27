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

namespace CodeCake
{

    public static class DotNetCoreRestoreSettingsExtension
    {
        public static T AddVersionArguments<T>(this T @this, SimpleRepositoryInfo info, Action<T> conf = null) where T : DotNetCoreSettings
        {
            var prev = @this.ArgumentCustomization;
            @this.ArgumentCustomization = args => (prev?.Invoke(args) ?? args)
                    .Append($@"/p:CakeBuild=""true""");

            if (info.IsValid)
            {
                var prev2 = @this.ArgumentCustomization;
                @this.ArgumentCustomization = args => (prev2?.Invoke(args) ?? args)
                        .Append($@"/p:Version=""{info.NuGetVersion}""")
                        .Append($@"/p:AssemblyVersion=""{info.MajorMinor}.0""")
                        .Append($@"/p:FileVersion=""{info.FileVersion}""")
                        .Append($@"/p:InformationalVersion=""{info.SemVer} ({info.NuGetVersion}) - SHA1: {info.CommitSha} - CommitDate: {info.CommitDateUtc.ToString("u")}""");
            }
            conf?.Invoke(@this);
            return @this;
        }
    }

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

            var releasesDir = Cake.Directory("CodeCakeBuilder/Releases");
            var coreBuildFile = Cake.File("CodeCakeBuilder/CoreBuild.proj");

            var projects = Cake.ParseSolution(solutionFileName)
                                       .Projects
                                       .Where(p => !(p is SolutionFolder)
                                                    && !p.Path.Segments.Contains( "IntegrationTests" )
                                                    && p.Name != "CodeCakeBuilder");

            // We publish .Tests projects for this solution.
            var projectsToPublish = projects;

            SimpleRepositoryInfo gitInfo = Cake.GetSimpleRepositoryInfo();

            // Configuration is either "Debug" or "Release".
            string configuration = null;

            Task("Check-Repository")
                .Does(() =>
                {
                    if (!gitInfo.IsValid)
                    {
                        if (Cake.IsInteractiveMode()
                            && Cake.ReadInteractiveOption("Repository is not ready to be published. Proceed anyway?", 'Y', 'N') == 'Y')
                        {
                            Cake.Warning("GitInfo is not valid, but you choose to continue...");
                        }
                        else throw new Exception("Repository is not ready to be published.");
                    }

                    configuration = gitInfo.IsValidRelease
                                    && (gitInfo.PreReleaseName.Length == 0 || gitInfo.PreReleaseName == "rc")
                                    ? "Release"
                                    : "Debug";

                    Cake.Information("Publishing {0} projects with version={1} and configuration={2}: {3}",
                        projectsToPublish.Count(),
                        gitInfo.SemVer,
                        configuration,
                        string.Join(", ", projectsToPublish.Select(p => p.Name)));
                });

            Task("Clean")
                .IsDependentOn("Check-Repository")
                .Does(() =>
                {
                    Cake.CleanDirectories(projects.Select(p => p.Path.GetDirectory().Combine("bin")));
                    Cake.CleanDirectories(releasesDir);
                    Cake.DeleteFiles("Tests/**/TestResult*.xml");
                });

            Task( "Restore-NuGet-Packages" )
                .IsDependentOn("Check-Repository")
                .Does( () =>
                {
                    Cake.DotNetCoreRestore(coreBuildFile, 
                        new DotNetCoreRestoreSettings().AddVersionArguments(gitInfo, c =>
                        {
                            // No impact see: https://github.com/NuGet/Home/issues/3772
                            // c.Verbosity = DotNetCoreRestoreVerbosity.Minimal;
                        }));
                });


            Task( "Build" )
                .IsDependentOn( "Clean" )
                .IsDependentOn( "Restore-NuGet-Packages" )
                .IsDependentOn( "Check-Repository" )
                .Does( () =>
                {
                    Cake.DotNetCoreBuild(coreBuildFile,
                        new DotNetCoreBuildSettings().AddVersionArguments(gitInfo, s =>
                        {
                            s.Configuration = configuration;
                        }));
                } );

            Task( "Unit-Testing" )
               .IsDependentOn( "Build" )
              .WithCriteria( () => !Cake.IsInteractiveMode()
                                      || Cake.ReadInteractiveOption( "Run unit tests?", 'Y', 'N' ) == 'Y' )
               .Does( () =>
               {
                   var testDlls = projects
                                    .Where( p => p.Name.EndsWith( ".Tests" ) )
                                    .Select( p => p.Path.GetDirectory().CombineWithFilePath( "bin/" + configuration + "/net451/" + p.Name + ".dll" ) );
                   Cake.Information( "Testing: {0}", string.Join( ", ", testDlls.Select( p => p.GetFilename().ToString() ) ) );
                   Cake.NUnit( testDlls, new NUnitSettings() { Framework = "v4.5" } );
               } );

            Task( "Create-NuGet-Packages" )
                .IsDependentOn( "Unit-Testing" )
                .Does( () =>
                {
                    Cake.CreateDirectory(releasesDir);
                    var settings = new DotNetCorePackSettings();
                    settings.ArgumentCustomization = args => args.Append("--include-symbols");
                    settings.NoBuild = true;
                    settings.Configuration = configuration;
                    settings.OutputDirectory = releasesDir;
                    settings.AddVersionArguments(gitInfo);
                    Cake.DotNetCorePack(coreBuildFile, settings);
                });

            Task( "Run-IntegrationTests" )
              .IsDependentOn( "Create-NuGet-Packages" )
              .WithCriteria( () => gitInfo.IsValid )
              .WithCriteria( () => !Cake.IsInteractiveMode()
                                      || Cake.ReadInteractiveOption( "Run integration tests?", 'Y', 'N' ) == 'Y' )
              .Does( () =>
              {
                  var integrationSolution = "IntegrationTests/IntegrationTests.sln";
                  var integrationProjects = Cake.ParseSolution(solutionFileName)
                                               .Projects
                                               .Where(p => !(p is SolutionFolder));
                  var integrationTests = integrationProjects.Where(p => p.Name.EndsWith(".Tests"));

                  Cake.DotNetCoreRestore(integrationSolution, new DotNetCoreRestoreSettings()
                  {
                      ArgumentCustomization = c => c.Append($@"/p:CKDBVersion=""{gitInfo.NuGetVersion}""")
                  });

                  Cake.DotNetCoreBuild(integrationSolution, new DotNetCoreBuildSettings()
                  {
                      ArgumentCustomization = c => c.Append($@"/p:CKDBVersion=""{gitInfo.NuGetVersion}""")
                  });

                  var testDlls = integrationTests
                                    .Select(p => p.Path.GetDirectory().CombineWithFilePath("bin/" + configuration + "/net451/" + p.Name + ".dll"));
                  Cake.Information("Testing: {0}", string.Join(", ", testDlls.Select(p => p.GetFilename().ToString())));
                  Cake.NUnit(testDlls, new NUnitSettings() { Framework = "v4.5" });
              });

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
                        if (Cake.AppVeyor().IsRunningOnAppVeyor)
                        {
                            Cake.AppVeyor().UpdateBuildVersion(gitInfo.SemVer);
                        }
                    });

            Task("Default").IsDependentOn("Push-NuGet-Packages" );
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
