using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPackages.Tests.From
{
    [TestFixture]
    public class UserTests : CK.DB.Actor.Tests.UserTests
    {
    }

    [TestFixture]
    public class GroupTests : CK.DB.Actor.Tests.GroupTests
    {
    }

    [TestFixture]
    public class AclSimpleTests : CK.DB.Acl.Tests.AclSimpleTests
    {
    }

    [TestFixture]
    public class AclTypeTests : CK.DB.Acl.AclType.Tests.AclTypeTests
    {
    }


}
