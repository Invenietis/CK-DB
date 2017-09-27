#if !NET461
using NUnit.Common;
using NUnitLite;
using System;
using System.Reflection;

namespace CK.DB.User.UserGoogle.RefreshToken.Tests
{
    public class Program
    {
        public static int Main( string[] args )
        {
            return new AutoRun( typeof( Program ).GetTypeInfo().Assembly )
                .Execute( args, new ExtendedTextWrapper( Console.Out ), Console.In );
        }

    }
}
#endif
