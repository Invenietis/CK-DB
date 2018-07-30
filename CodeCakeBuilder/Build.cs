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
    [AddPath( "%UserProfile%/.nuget/packages/**/tools*" )]
    public partial class Build : CodeCakeHost
    {
        public Build()
        {
            Cake.Log.Verbosity = Verbosity.Diagnostic;

            const string solutionName = "CK-DB";
            const string solutionFileName = solutionName + ".sln";

            var releasesDir = Cake.Directory( "CodeCakeBuilder/Releases" );

            var projects = Cake.ParseSolution( solutionFileName )
                                       .Projects
                                       .Where( p => !(p is SolutionFolder)
                                                     && p.Name != "CodeCakeBuilder" );

            // We publish .Tests projects for this solution.
            var projectsToPublish = projects;

            // The SimpleRepositoryInfo should be computed once and only once.
            SimpleRepositoryInfo gitInfo = Cake.GetSimpleRepositoryInfo();
            // This default global info will be replaced by Check-Repository task.
            // It is allocated here to ease debugging and/or manual work on complex build script.
            CheckRepositoryInfo globalInfo = new CheckRepositoryInfo { Version = gitInfo.SafeNuGetVersion };

            Task( "Check-Repository" )
               .Does( () =>
               {
                   globalInfo = StandardCheckRepository( projectsToPublish, gitInfo );
                   if( globalInfo.ShouldStop )
                   {
                       Cake.TerminateWithSuccess( "All packages from this commit are already available. Build skipped." );
                   }
               } );

            Task( "Clean" )
                .Does( () =>
                 {
                     Cake.CleanDirectories( projects.Select( p => p.Path.GetDirectory().Combine( "bin" ) ) );
                     Cake.CleanDirectories( releasesDir );
                     Cake.DeleteFiles( "Tests/**/TestResult*.xml" );
                 } );

            Task( "Build" )
                .IsDependentOn( "Check-Repository" )
                .IsDependentOn( "Clean" )
                .Does( () =>
                {
                    StandardSolutionBuild( solutionFileName, gitInfo, globalInfo.BuildConfiguration );
                } );

            Task( "Unit-Testing" )
                .IsDependentOn( "Build" )
                .WithCriteria( () => Cake.InteractiveMode() == InteractiveMode.NoInteraction
                                     || Cake.ReadInteractiveOption( "RunUnitTests", "Run Unit Tests?", 'Y', 'N' ) == 'Y' )
               .Does( () =>
               {
                   var testProjects = projects.Where( p => p.Name.EndsWith( ".Tests" ) );
                   StandardUnitTests( globalInfo.BuildConfiguration, testProjects );
               } );

            Task( "Create-NuGet-Packages" )
                .IsDependentOn( "Unit-Testing" )
                .Does( () =>
                {
                    StandardCreateNuGetPackages( releasesDir, projectsToPublish, gitInfo, globalInfo.BuildConfiguration );
                } );


            Task( "Push-NuGet-Packages" )
                    .IsDependentOn( "Create-NuGet-Packages" )
                    .WithCriteria( () => gitInfo.IsValid )
                    .Does( () =>
                    {
                        StandardPushNuGetPackages( globalInfo, releasesDir );
                    } );

            Task( "Default" ).IsDependentOn( "Push-NuGet-Packages" );
        }
    }
}
