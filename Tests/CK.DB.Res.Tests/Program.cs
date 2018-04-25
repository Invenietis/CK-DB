#if !NET461
using CK.Monitoring;
using NUnit.Common;
using NUnitLite;
using System;
using System.Reflection;

namespace CK.DB.Res.Tests
{
    public class Program
    {
        public static int Main( string[] args )
        {
            int result = new AutoRun( Assembly.GetEntryAssembly() )
                .Execute( args, new ExtendedTextWrapper( Console.Out ), Console.In );
            GrandOutput.Default?.Dispose();
            return result;
        }
    }
}

#endif
