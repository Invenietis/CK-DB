using System;
using System.Collections.Generic;
using System.Text;

namespace CK.DB.Auth
{
    /// <summary>
    /// Captures the result of <see cref="IGenericAuthenticationProvider.CreateOrUpdateUser"/>
    /// or <see cref="IGenericAuthenticationProvider.CreateOrUpdateUserAsync"/>.
    /// </summary>
    public struct CreateOrUpdateResult
    {
        /// <summary>
        /// The <see cref="LoginResult"/>.
        /// Meaningful only when <see cref="CreateOrUpdateMode.WithLogin"/> is used.
        /// </summary>
        public readonly LoginResult LoginResult;

        /// <summary>
        /// The create/update operation result.
        /// </summary>
        public readonly CreateOrUpdateOperationResult OperationResult;

        /// <summary>
        /// Initializes a new <see cref="CreateOrUpdateResult"/>.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <param name="result">Operation result.</param>
        /// <param name="failureReason">Optional login failure reason.</param>
        /// <param name="failureCode">Optional login failure error code.</param>
        public CreateOrUpdateResult( int userId, CreateOrUpdateOperationResult result, string failureReason, int? failureCode )
        {
            LoginResult = new LoginResult( userId, failureReason, failureCode );
            OperationResult = result;
        }
    }

}
