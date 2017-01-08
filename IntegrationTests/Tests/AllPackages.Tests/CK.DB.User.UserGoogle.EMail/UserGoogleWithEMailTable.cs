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

namespace CK.DB.User.UserGoogle.EMail
{
    [SqlTable( "CK.tUserGoogle", ResourceType = typeof(UserGoogleWithEMailTable), ResourcePath = "~AllPackages.Tests.CK.DB.User.UserGoogle.EMail.Res" )]
    [Versions("1.0.0")]
    public abstract class UserGoogleWithEMailTable : UserGoogleTable
    {
        protected override StringBuilder AppendColumns( StringBuilder b )
        {
            return base.AppendColumns( b ).Append( "EMail, EMailVerified" );
        }

        public override UserGoogleInfo CreateUserInfo() => new UserGoogleInfoWithMail();

        protected override int FillUserGoogleInfo( UserGoogleInfo info, SqlDataReader r, int idx )
        {
            idx = base.FillUserGoogleInfo( info, r, idx );
            ((UserGoogleInfoWithMail)info).EMail = r.GetString( idx++ );
            ((UserGoogleInfoWithMail)info).EMailVerified = r.GetBoolean( idx++ );
            return idx;
        }

        protected override RawResult RawCreateOrUpdateGoogleUser( ISqlCallContext ctx, int actorId, int userId, UserGoogleInfo info, CreateOrUpdateMode mode )
        {
            return RawCreateOrUpdateGoogleUser( ctx, actorId, userId, (UserGoogleInfoWithMail)info, mode );
        }

        [SqlProcedure( "transform:sUserGoogleCreateOrUpdate" )]
        protected abstract RawResult RawCreateOrUpdateGoogleUser( ISqlCallContext ctx, int actorId, int userId, [ParameterSource]UserGoogleInfoWithMail info, CreateOrUpdateMode mode );

        protected override Task<RawResult> RawCreateOrUpdateGoogleUserAsync( ISqlCallContext ctx, int actorId, int userId, UserGoogleInfo info, CreateOrUpdateMode mode, CancellationToken cancellationToken )
        {
            return RawCreateOrUpdateGoogleUserAsync( ctx, actorId, userId, (UserGoogleInfoWithMail)info, mode, cancellationToken );
        }

        [SqlProcedure( "transform:sUserGoogleCreateOrUpdate" )]
        protected abstract Task<RawResult> RawCreateOrUpdateGoogleUserAsync( ISqlCallContext ctx, int actorId, int userId, [ParameterSource]UserGoogleInfoWithMail info, CreateOrUpdateMode mode, CancellationToken cancellationToken );

    }
}
