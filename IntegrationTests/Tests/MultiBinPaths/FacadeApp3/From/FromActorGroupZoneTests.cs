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
    public class ZoneTests : CK.DB.Zone.Tests.ZoneTests
    {
    }

    [TestFixture]
    public class HZoneSimpleTests : CK.DB.HZone.Tests.HZoneSimpleTests
    {
    }

    [TestFixture]
    public class ZoneSameBehaviorTests : CK.DB.HZone.Tests.ZoneSameBehaviorTests
    {
    }

    [TestFixture]
    public class GroupNameTests : CK.DB.Group.SimpleNaming.Tests.GroupNameTests
    {
    }

    [TestFixture]
    public class ZoneNameTests : CK.DB.Zone.SimpleNaming.Tests.ZoneNameTests
    {
    }
}
