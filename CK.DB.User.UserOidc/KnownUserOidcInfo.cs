using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.DB.User.UserOidc
{
    /// <summary>
    /// Associates a user identifier to its Oidc user information.
    /// </summary>
    public class KnownUserOidcInfo
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the associated information.
        /// Since <see cref="IUserOidcInfo"/> is a <see cref="IPoco"/>, this can be casted
        /// to any specialized info interface defined by other packages.
        /// </summary>
        public IUserOidcInfo Info { get; set; }
    }
}
