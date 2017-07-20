using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.DB.User.UserGoogle
{
    /// <summary>
    /// Associates a user identifier to its Google user information.
    /// </summary>
    public class KnownUserGoogleInfo
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the associated information.
        /// Since <see cref="IUserGoogleInfo"/> is a <see cref="IPoco"/>, this can be casted
        /// to any specialized info interface defined by other packages.
        /// </summary>
        public IUserGoogleInfo Info { get; set; }
    }
}
