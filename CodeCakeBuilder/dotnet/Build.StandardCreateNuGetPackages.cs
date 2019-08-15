using System;

namespace CodeCake
{
    public partial class Build
    {
        [Obsolete( "Use DotnetSolution.Pack() instead")]
        void StandardCreateNuGetPackages( StandardGlobalInfo globalInfo )
        {
            globalInfo.GetDotnetSolution().Pack();
        }
        [Obsolete( "Use DotnetSolution.Pack() instead") ]
        void StandardCreateNuGetPackages( NuGetArtifactType nugetInfo )
        {
            nugetInfo.GlobalInfo.GetDotnetSolution().Pack();
        }
    }
}
