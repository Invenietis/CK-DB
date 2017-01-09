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
        /// </summary>
        public IUserGoogleInfo Info {get;set; }
    }
}
