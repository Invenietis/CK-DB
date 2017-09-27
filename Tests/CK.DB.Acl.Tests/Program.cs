#if !NET461
using NUnit.Common;
using NUnitLite;
using System;
using System.Reflection;

namespace CK.DB.Acl.Tests
{
    public class Program
    {
        public static int Main( string[] args )
        {
            return new AutoRun( Assembly.GetEntryAssembly() )
                .Execute( args, new ExtendedTextWrapper( Console.Out ), Console.In );
        }

    }
}
#endif
