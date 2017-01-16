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
    [SqlTable( "CK.tUserGoogle", ResourceType = typeof(UserGoogleWithEMailTable), ResourcePath = "Res" )]
    [Versions("1.0.0")]
    public abstract class UserGoogleWithEMailTable : UserGoogleTable
    {
        protected override StringBuilder AppendColumns( StringBuilder b )
        {
            return base.AppendColumns( b ).Append( ", EMail, EMailVerified" );
        }

        protected override int FillUserGoogleInfo( IUserGoogleInfo info, SqlDataReader r, int idx )
        {
            idx = base.FillUserGoogleInfo( info, r, idx );
            ((IUserGoogleInfoWithMail)info).EMail = r.GetString( idx++ );
            ((IUserGoogleInfoWithMail)info).EMailVerified = r.GetBoolean( idx++ );
            return idx;
        }

        protected override RawResult RawCreateOrUpdateGoogleUser( ISqlCallContext ctx, int actorId, int userId, IUserGoogleInfo info, CreateOrUpdateMode mode )
        {
            return RawCreateOrUpdateGoogleUser( ctx, actorId, userId, (IUserGoogleInfoWithMail)info, mode );
        }

        [SqlProcedure( "transform:sUserGoogleCreateOrUpdate" )]
        protected abstract RawResult RawCreateOrUpdateGoogleUser( ISqlCallContext ctx, int actorId, int userId, [ParameterSource]IUserGoogleInfoWithMail info, CreateOrUpdateMode mode );

        protected override Task<RawResult> RawCreateOrUpdateGoogleUserAsync( ISqlCallContext ctx, int actorId, int userId, IUserGoogleInfo info, CreateOrUpdateMode mode, CancellationToken cancellationToken )
        {
            return RawCreateOrUpdateGoogleUserAsync( ctx, actorId, userId, (IUserGoogleInfoWithMail)info, mode, cancellationToken );
        }

        [SqlProcedure( "transform:sUserGoogleCreateOrUpdate" )]
        protected abstract Task<RawResult> RawCreateOrUpdateGoogleUserAsync( ISqlCallContext ctx, int actorId, int userId, [ParameterSource]IUserGoogleInfoWithMail info, CreateOrUpdateMode mode, CancellationToken cancellationToken );

    }
}
