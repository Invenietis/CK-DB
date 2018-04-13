#if !NET461
using NUnit.Common;
using NUnitLite;
using System;
using System.Reflection;

namespace CK.DB.Acl.AclType.Tests
{
    public class Program
    {
        public static int Main( string[] args ) => CK.DB.Actor.Tests.Program.Main( args );

    }
}
#endif
