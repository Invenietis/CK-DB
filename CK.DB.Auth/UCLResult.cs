using System;
using System.Collections.Generic;
using System.Text;

namespace CK.DB.Auth
{
    /// <summary>
    /// Captures the result of <see cref="IGenericAuthenticationProvider.CreateOrUpdateUser"/>
    /// or <see cref="IGenericAuthenticationProvider.CreateOrUpdateUserAsync"/>.
    /// </summary>
    public struct UCLResult
    {
        /// <summary>
        /// The <see cref="LoginResult"/>.
        /// Meaningful only when <see cref="UCLMode.WithCheckLogin"/> is used:
        /// <see cref="LoginResult.IsEmpty"/> is true if login was not challenged.
        /// </summary>
        public readonly LoginResult LoginResult;

        /// <summary>
        /// The update or create operation result.
        /// </summary>
        public readonly UCResult OperationResult;

        /// <summary>
        /// Initializes a new <see cref="UCLResult"/> from a database call.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <param name="ucResult">Operation result.</param>
        /// <param name="loginFailureReason">Optional login failure reason.</param>
        /// <param name="loginFailureCode">Optional login failure error code.</param>
        public UCLResult( int userId, UCResult ucResult, string loginFailureReason, int? loginFailureCode )
        {
            OperationResult = ucResult;
            LoginResult = new LoginResult( userId, loginFailureReason, loginFailureCode );
        }
    }

}
