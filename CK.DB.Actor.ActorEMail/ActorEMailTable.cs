using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.SqlServer.Setup;
using CK.Setup;
using CK.SqlServer;
using System.Threading.Tasks;

namespace CK.DB.Actor.ActorEMail
{
    /// <summary>
    /// Holds mails (among them one is the primary email) for a user or a group.
    /// </summary>
    [SqlTable( "tActorEMail", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    [SqlObjectItem( "transform:sUserDestroy, transform:sGroupDestroy, transform:vUser, transform:vGroup" )]
    public abstract partial class ActorEMailTable : SqlTable
    {
        /// <summary>
        /// Adds an email to a user or a group and/or sets whether it is the primary one.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userOrGroupId">The user or group identifier for which an email should be added or configured as the primary one.</param>
        /// <param name="email">The email.</param>
        /// <param name="isPrimary">True to set the email as the user or group's primary one.</param>
        /// <param name="validate">Optionaly sets the ValTime of the email: true to set it to sysUTCDateTime(), false to reset it to '0001:01:01T00:00:00.00'.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sActorEMailAdd" )]
        public abstract Task AddEMailAsync( ISqlCallContext ctx, int actorId, int userOrGroupId, string email, bool isPrimary, bool? validate = null );

        /// <summary>
        /// Removes an email from the user or group's emails (removing an unexisting email is silently ignored).
        /// When the removed email is the primary one, the most recently validated email becomes
        /// the new primary one.
        /// By default, this procedure allows the removal of the only actor's email.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userOrGroupId">The user or group identifier for which an email must be removed.</param>
        /// <param name="email">The email to remove.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sActorEMailRemove" )]
        public abstract Task RemoveEMailAsync( ISqlCallContext ctx, int actorId, int userOrGroupId, string email );

        /// <summary>
        /// Validates an email by uptating its ValTime to the current sysutdatetime.
        /// Validating a non existing email is silently ignored.
        /// If the current primary mail is not validated, this newly validated email becomes
        /// the primary one.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userOrGroupId">The user or group identifier for which the email is valid.</param>
        /// <param name="email">The email to valid.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sActorEMailValidate" )]
        public abstract Task ValidateEMailAsync( ISqlCallContext ctx, int actorId, int userOrGroupId, string email );

    }
}
