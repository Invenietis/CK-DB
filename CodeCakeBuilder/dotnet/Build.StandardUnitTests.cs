using Cake.Common.Solution;
using System;
using System.Collections.Generic;

namespace CodeCake
{
    public partial class Build
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="globalInfo"></param>
        /// <param name="testProjects"></param>
        /// <param name="useNUnit264ForNet461">Will use nunit 264 for Net461 project if true.</param>
        [Obsolete( "Use DotnetSolution.Test() instead" )]
        void StandardUnitTests( StandardGlobalInfo globalInfo, IEnumerable<SolutionProject> testProjects )
        {
            globalInfo.GetDotnetSolution().Test();
        }
    }
}
