using Cake.Common;
using Cake.Common.Solution;
using Cake.Common.IO;
using Cake.Common.Tools.MSBuild;
using Cake.Common.Tools.NuGet;
using Cake.Core;
using Cake.Common.Diagnostics;
using Cake.Common.Tools.NuGet.Pack;
using System.Linq;
using Cake.Core.Diagnostics;
using Cake.Common.Tools.NuGet.Restore;
using System;
using Cake.Common.Tools.NuGet.Push;
using Cake.Common.Tools.NUnit;

namespace CodeCake
{
    [AddPath( "../CodeCakeBuilder/Tools" )] 
    [AddPath( "packages/**/tools*" )]    
    public class Build : CodeCakeHost
    {
        public Build()
        {
            var configuration = "Debug";
            string solutionFile = "IntegrationTests.sln";
            var solution = Cake.ParseSolution( solutionFile );

            Task( "Clean" )
                .Does( () =>
                {
                    // Avoids cleaning CodeCakeBuilder itself!
                    Cake.CleanDirectories( "**/bin/" + configuration, d => !d.Path.Segments.Contains( "CodeCakeBuilder" ) );
                    Cake.CleanDirectories( "**/obj/" + configuration, d => !d.Path.Segments.Contains( "CodeCakeBuilder" ) );
                } );

            Task( "Restore-NuGet-Packages" )
                .Does( () =>
                {
                    Cake.NuGetRestore( solutionFile );
                } );

            Task( "Build" )
                .IsDependentOn( "Clean" )
                .IsDependentOn( "Restore-NuGet-Packages" )
                .Does( () =>
                {
                    using( var tempSln = Cake.CreateTemporarySolutionFile( solutionFile ) )
                    {
                        // Excludes "CodeCakeBuilder" itself from compilation!
                        tempSln.ExcludeProjectsFromBuild( "CodeCakeBuilder" );
                        Cake.MSBuild( tempSln.FullPath, new MSBuildSettings()
                                .SetConfiguration( configuration )
                                .SetVerbosity( Verbosity.Minimal ) );
                    }
                } );

            Task( "Unit-Testing" )
               .IsDependentOn( "Build" )
               .Does( () =>
               {
                   var testDlls = solution.Projects
                                            .Where( p => p.Name.EndsWith( ".Tests" ) )
                                            .Select( p => p.Path.GetDirectory().CombineWithFilePath( "bin/" + configuration + "/" + p.Name + ".dll" ) );
                   Cake.Information( "Testing: {0}", string.Join( ", ", testDlls.Select( p => p.GetFilename().ToString() ) ) );
                   Cake.NUnit( testDlls, new NUnitSettings() { Framework = "v4.5" } );
               } );

            Task( "Default" ).IsDependentOn( "Unit-Testing" );

        }
    }
}
