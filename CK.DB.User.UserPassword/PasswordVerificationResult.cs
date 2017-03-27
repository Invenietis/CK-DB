using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.User.UserPassword
{
    /// <summary>
    /// Specifies the results for password verification.
    /// </summary>
    /// <remarks>
    /// Not to be confused with ASP.NET Identity's PasswordVerificationResult.
    /// More information: https://github.com/aspnet/Identity/blob/master/src/Microsoft.AspNetCore.Identity/PasswordVerificationResult.cs
    /// </remarks>
    public enum PasswordVerificationResult
    {
        /// <summary>
        /// Indicates password verification failed.
        /// </summary>
        Failed,

        /// <summary>
        /// Indicates password verification was successful however the password was encoded using a deprecated algorithm
        /// and should be rehashed and updated.
        /// </summary>
        SuccessRehashNeeded,

        /// <summary>
        /// Indicates password verification was successful.
        /// </summary>
        Success
    }
}
