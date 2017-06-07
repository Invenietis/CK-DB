using System;
using System.Collections.Generic;
using System.Text;

namespace CK.DB.Auth
{
    /// <summary>
    /// Scheme name and last used time for <see cref="IUserAuthInfo"/>.
    /// This struct is immutable.
    /// </summary>
    public struct UserAuthSchemeInfo
    {
        /// <summary>
        /// Initializes a new <see cref="UserAuthSchemeInfo"/>.
        /// </summary>
        /// <param name="s">Scheme name.</param>
        /// <param name="t">Last used time.</param>
        public UserAuthSchemeInfo(string s, DateTime t)
        {
            Name = s;
            LastUsed = t;
        }

        /// <summary>
        /// The scheme name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The last time this provider has been used.
        /// </summary>
        public readonly DateTime LastUsed;
    }
}
