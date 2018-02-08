using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPackages.Tests.From
{
    [TestFixture]
    public class AuthTests : CK.DB.Auth.Tests.AuthTests
    {
    }

    [TestFixture]
    public class AuthScopeSetTests : CK.DB.Auth.AuthScope.Tests.AuthScopeSetTests
    {
    }

    [TestFixture]
    public class UserGoogleAuthScopeTests : CK.DB.User.UserGoogle.AuthScope.Tests.UserGoogleAuthScopeTests
    {
    }

    [TestFixture]
    public class UserGoogleEMailColumnsTests : CK.DB.User.UserGoogle.EMailColumns.Tests.UserGoogleEMailTests
    {
    }

    [TestFixture]
    public class UserGoogleRefreshTokenTests : CK.DB.User.UserGoogle.RefreshToken.Tests.UserGoogleRefreshTokenTests
    {
    }

    [TestFixture]
    public class UserSimpleCodeTests : CK.DB.User.UserSimpleCode.Tests.UserSimpleCodeTests
    {
    }

}
