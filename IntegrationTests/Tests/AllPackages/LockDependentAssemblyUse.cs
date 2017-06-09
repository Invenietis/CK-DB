using System;
using System.Collections.Generic;
using System.Text;

namespace AllPackages
{
    public static class LockDependentAssemblyUse
    {
        public static IEnumerable<Type> Types = new[] {
            typeof(CK.DB.Acl.Package),
            typeof(CK.DB.Acl.AclType.Package),
            typeof(CK.DB.Actor.Package),
            typeof(CK.DB.Actor.ActorEMail.Package),
            typeof(CK.DB.Auth.Package),
            typeof(CK.DB.Auth.AuthScope.Package),
            typeof(CK.DB.Culture.Package),
            typeof(CK.DB.HZone.Package),
            typeof(CK.DB.Res.Package),
            typeof(CK.DB.Res.MCResHtml.Package),
            typeof(CK.DB.Res.MCResString.Package),
            typeof(CK.DB.Res.MCResText.Package),
            typeof(CK.DB.Res.ResHtml.Package),
            typeof(CK.DB.Res.ResHtml.Package),
            typeof(CK.DB.Res.ResName.Package),
            typeof(CK.DB.Res.ResString.Package),
            typeof(CK.DB.Res.ResText.Package),
            typeof(CK.DB.User.UserGoogle.Package),
            typeof(CK.DB.User.UserGoogle.AuthScope.Package),
            typeof(CK.DB.User.UserGoogle.EMailColumns.Package),
            typeof(CK.DB.User.UserGoogle.RefreshToken.Package),
            typeof(CK.DB.User.UserOidc.Package),
            typeof(CK.DB.User.UserPassword.Package),
            typeof(CK.DB.Zone.Package),
            typeof(CK.DB.Zone.SimpleNaming.Package),
        };
    }
}
