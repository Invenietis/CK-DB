using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Auth
{
    /// <summary>
    /// Defines the status of each scope in a scope set.
    /// </summary>
    public enum ScopeWARStatus
    {
        /// <summary>
        /// The scope is waiting the user consent.
        /// </summary>
        Waiting,
        /// <summary>
        /// The scope has been accepted by the user.
        /// </summary>
        Accepted,
        /// <summary>
        /// The scope consent has been rejected by the user.
        /// </summary>
        Rejected
    }
}
