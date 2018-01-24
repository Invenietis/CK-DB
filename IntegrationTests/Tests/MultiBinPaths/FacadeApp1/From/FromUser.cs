using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPackages.Tests.From
{
    [TestFixture]
    public class UserPasswordTests : CK.DB.User.UserPassword.Tests.UserPasswordTests
    {
    }

    [TestFixture]
    public class UserGoogleTests : CK.DB.User.UserGoogle.Tests.UserGoogleTests
    {
    }

    [TestFixture]
    public class UserOidcTests : CK.DB.User.UserOidc.Tests.UserOidcTests
    {
    }
}
