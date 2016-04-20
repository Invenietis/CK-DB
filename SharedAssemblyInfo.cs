using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany( "Invenietis" )]
[assembly: AssemblyProduct( "CK-DB" )]
[assembly: AssemblyCopyright( "Copyright © Invenietis 2012-2016" )]
[assembly: AssemblyTrademark( "" )]
[assembly: CLSCompliant( true )]

#if DEBUG
    [assembly: AssemblyConfiguration( "Debug" )]
#else
[assembly: AssemblyConfiguration( "Release" )]
#endif
