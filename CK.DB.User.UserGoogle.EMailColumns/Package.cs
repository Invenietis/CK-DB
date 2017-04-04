using CK.DB.User.UserGoogle;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using CK.DB.Auth;
using CK.SqlServer;
using System.Threading;
using CK.Setup;

namespace CK.DB.User.UserGoogle.EMailColumns
{
    /// <summary>
    /// Package that adds EMails columns support to Google authentication.
    /// </summary>
    [SqlPackage( ResourcePath = "Res" )]
    [Versions("1.0.0")]
    [SqlObjectItem( "transform:CK.sUserGoogleCreateOrUpdate" )]
    public abstract class Package : SqlPackage
    {
        void StObjConstruct( UserGoogle.UserGoogleTable table )
        {
        }
    }
}
