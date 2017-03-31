using System;
using System.Collections.Generic;
using System.Text;

namespace CK.DB.Auth
{
    /// <summary>
    /// Provider name and last used time for <see cref="IUserAuthInfo"/>.
    /// This struct is immutable.
    /// </summary>
    public struct UserAuthProviderInfo
    {
        /// <summary>
        /// Initializes a new <see cref="UserAuthProviderInfo"/>.
        /// </summary>
        /// <param name="n">Provider name.</param>
        /// <param name="t">Last used time.</param>
        public UserAuthProviderInfo(string n, DateTime t)
        {
            Name = n;
            LastUsed = t;
        }

        /// <summary>
        /// The provider name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The last time this provider has been used.
        /// </summary>
        public readonly DateTime LastUsed;
    }
}
