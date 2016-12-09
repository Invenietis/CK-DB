using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPackages.Tests.From
{
    [TestFixture]
    public class AuthScopeSetTests : CK.DB.Auth.AuthScope.Tests.AuthScopeSetTests
    {
    }

    [TestFixture]
    public class UserGoogleAuthScopeTests : CK.DB.User.UserGoogle.AuthScope.Tests.UserGoogleAuthScopeTests
    {
    }

}
