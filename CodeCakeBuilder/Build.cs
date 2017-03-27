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
                    Cake.DotNetCoreRestore(coreBuildFile, new DotNetCoreRestoreSettings().AddVersionArguments(gitInfo));
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
              .WithCriteria( () => gitInfo.IsValid )
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
                //  var integrationSolution = "IntegrationTests/IntegrationTests.sln";
                //  var integration = Cake.ParseSolution( integrationSolution );
                //  var projects = integration.Projects
                //                              .Where( p => p.Name != "CodeCakeBuilder" )
                //                              .Select( p => new
                //                              {
                //                                  CSProj = p.Path.FullPath,
                //                                  ConfigFile = p.Path.GetDirectory().CombineWithFilePath( "packages.config" ).FullPath
                //                              } )
                //                              .Where( p => System.IO.File.Exists( p.ConfigFile ) );
                //// Cleans all the existing IntegrationTests/packages.
                //// The CodeCakeBuilder restore will get them (from Release for CK-DB packages).
                //Cake.CleanDirectory( "IntegrationTests/packages" );

                //  foreach( var config in projects.Select( p => p.ConfigFile ) )
                //  {
                //      XDocument doc = XDocument.Load( config );
                //      int countRef = 0;
                //      foreach( var p in doc.Root.Elements( "package" ) )
                //      {
                //          string packageName = p.Attribute( "id" ).Value;
                //          if( IntegrationDependentPackages.ContainsKey( packageName ) )
                //          {
                //              string depVersion = IntegrationDependentPackages[packageName];
                //              string curVersion = p.Attribute( "version" ).Value;
                //              if( curVersion != depVersion )
                //              {
                //                  p.SetAttributeValue( "version", depVersion );
                //                  Cake.Information( $"=> package.config: {packageName}: {curVersion} -> {depVersion}." );
                //                  ++countRef;
                //              }
                //          }
                //      }
                //      if( countRef > 0 )
                //      {
                //          Cake.Information( $"Updated {countRef} in file {config}." );
                //          doc.Save( config );
                //      }
                //  }
                //  foreach( var csproj in projects.Select( p => p.CSProj ) )
                //  {
                //      XDocument doc = XDocument.Load( csproj );
                //      int countRef = 0;
                //      var projection = doc.Root.Descendants( msBuild + "Reference" )
                //                              .Select( e => new
                //                              {
                //                                  Reference = e,
                //                                  IncludeAttr = e.Attribute( "Include" ),
                //                                  HintPathElement = e.Element( msBuild + "HintPath" ),
                //                              } );
                //      var filtered = projection.Where( e => e.HintPathElement != null
                //                                              && e.IncludeAttr != null
                //                                              && e.HintPathElement.Value.StartsWith( @"..\..\packages\" ) );
                //      var final = filtered.Select( e => new
                //      {
                //          E = e,
                //          ProjectName = new AssemblyName( e.IncludeAttr.Value ).Name
                //      } )
                //                      .Where( e => IntegrationDependentPackages.ContainsKey( e.ProjectName ) );

                //      foreach( var p in final )
                //      {
                //          var version = IntegrationDependentPackages[p.ProjectName];
                //          var path = p.E.HintPathElement.Value.Split( '\\' );
                //          var newFolder = p.ProjectName + '.' + version;
                //          var curFolder = path[3];
                //          if( curFolder != newFolder )
                //          {
                //              path[3] = newFolder;
                //              p.E.HintPathElement.Value = string.Join( "\\", path );
                //              Cake.Information( $"=> cproj: {p.ProjectName}: {curFolder} -> {newFolder}." );
                //              ++countRef;
                //          }
                //      }
                //      if( countRef > 0 )
                //      {
                //          Cake.Information( $"Updated {countRef} references in file {csproj}." );
                //          doc.Save( csproj );
                //      }
                //  }

                //  Cake.NuGetRestore( integrationSolution );
                //  Cake.MSBuild( "IntegrationTests/CodeCakeBuilder/CodeCakeBuilder.csproj", settings =>
                //  {
                //      settings.Configuration = configuration;
                //      settings.Verbosity = Verbosity.Minimal;
                //  } );
                //  if( Cake.StartProcess( $"IntegrationTests/CodeCakeBuilder/bin/{configuration}/CodeCakeBuilder.exe", "-" + InteractiveAliases.NoInteractionArgument ) != 0 )
                //  {
                //      Cake.TerminateWithError( "Error in IntegrationTests." );
                //  }
              } );


            Task( "Push-NuGet-Packages" )
                    .IsDependentOn( "Create-NuGet-Packages" )
                    //.IsDependentOn( "Run-IntegrationTests" )
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

            Task("Migrate-From-Old")
                    .Does(() =>
                    {
                        XNamespace ns = "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd";
                        var sourceSolutionFileName = Cake.File("../CK-DB-Old/CK-DB.sln");
                        var docProj = XDocument.Load(Cake.File("CK.DB.Actor/CK.DB.Actor.csproj"));
                        var docTestProj = XDocument.Load(Cake.File("Tests/CK.DB.Actor.Tests/CK.DB.Actor.Tests.csproj"));
                        var sourceProjects = Cake.ParseSolution(sourceSolutionFileName)
                                                  .Projects
                                                  .Where(p => !(p is SolutionFolder)
                                                               && !p.Path.Segments.Contains("IntegrationTests")
                                                               && p.Name != "CodeCakeBuilder");
                        foreach ( var p in sourceProjects)
                        {
                            if (p.Name == "CK.DB.Actor" || p.Name == "CK.DB.Actor.Tests") continue;
                            bool isTest = p.Name.EndsWith(".Tests");
                            var sourceDir = p.Path.GetDirectory().FullPath;
                            var targetDir = sourceDir.Replace("CK-DB-Old", "CK-DB");
                            Cake.CleanDirectories(System.IO.Path.Combine(sourceDir, "bin"));
                            Cake.CleanDirectories(System.IO.Path.Combine(sourceDir, "obj"));
                            Cake.CopyDirectory(sourceDir, targetDir);
                            if (!isTest) Cake.DeleteFile(System.IO.Path.Combine(targetDir, "app.config"));
                            Cake.DeleteFile(System.IO.Path.Combine(targetDir, "packages.config"));
                            Cake.DeleteFile(System.IO.Path.Combine(targetDir, "Properties", "AssemblyInfo.cs"));
                            var nuSpec = XDocument.Load(Cake.File("CodeCakeBuilder/NuSpec/"+p.Name+".nuspec"));
                            var desc = nuSpec.Root.Elements(ns+"metadata").Elements(ns+"description").FirstOrDefault()?.Value?.Trim();
                            var projectDependencies = nuSpec.Root.Descendants(ns+"dependency")
                                                        .Where(e => (string)e.Attribute("version") == "$version$")
                                                        .Select(e => (string)e.Attribute("id"));
                            if ( !isTest )
                            {
                                var csproj = new XDocument(docProj);
                                csproj.Root.Descendants("Description").Single().Value = desc;
                                if( p.Name != "CK.DB.Auth.AuthScope" 
                                    && p.Name != "CK.DB.Culture"
                                    && p.Name != "CK.DB.Res")
                                {
                                    var refs = csproj.Root.Elements("ItemGroup").Elements("PackageReference").First().Parent;
                                    refs.Elements().Remove();
                                    refs.Add(projectDependencies.Select(name => new XElement("ProjectReference", new XAttribute("Include", "..\\"+name+"\\"+name+".csproj"))));
                                }
                                csproj.Save(System.IO.Path.Combine(targetDir, p.Name + ".csproj"));
                            }
                            else
                            {
                                var csproj = new XDocument(docTestProj);
                                csproj.Root.Descendants("Description").Single().Value = desc;
                                if (p.Name != "CK.DB.Auth.AuthScope.Tests"
                                    && p.Name != "CK.DB.Culture.Tests"
                                    && p.Name != "CK.DB.Res.Tests")
                                {
                                    var refs = csproj.Root.Elements("ItemGroup").Elements("PackageReference").First().Parent;
                                    refs.Elements().Remove();
                                    refs.Add(projectDependencies.Select(name => new XElement("ProjectReference", new XAttribute("Include", 
                                        name.EndsWith(".Tests") 
                                            ? "..\\" + name + "\\" + name + ".csproj"
                                            : "..\\..\\" + name + "\\" + name + ".csproj" ))));
                                }
                                csproj.Save(System.IO.Path.Combine(targetDir, p.Name + ".csproj"));
                            }
                        }

                    });

            //Task("Default").IsDependentOn("Push-NuGet-Packages" );
            Task("Default").IsDependentOn("Migrate-From-Old" );
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
