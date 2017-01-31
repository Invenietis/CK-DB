using CK.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.User.UserPassword
{
    /// <summary>
    /// Provides an abstraction for hashing passwords.
    /// </summary>
    /// <remarks>
    /// Not to be confused with ASP.NET Identity's IPasswordHasher&lt;TUser&gt;.
    /// More information: https://github.com/aspnet/Identity/blob/master/src/Microsoft.AspNetCore.Identity/IPasswordHasher.cs
    /// </remarks>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Returns a hashed representation in bytes of the supplied <paramref name="password"/>.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>A hashed representation in bytes of the supplied <paramref name="password"/>.</returns>
        byte[] HashPassword( string password );

        /// <summary>
        /// Returns a <see cref="PasswordVerificationResult"/> indicating the result of a password hash comparison.
        /// </summary>
        /// <param name="hashedPassword">The hash value for a user's stored password.</param>
        /// <param name="providedPassword">The password supplied for comparison.</param>
        /// <returns>A <see cref="PasswordVerificationResult"/> indicating the result of a password hash comparison.</returns>
        PasswordVerificationResult VerifyHashedPassword( byte[] hashedPassword, string providedPassword );
    }
}
