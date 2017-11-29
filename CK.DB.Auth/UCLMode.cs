using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Auth
{
    /// <summary>
    /// Defines <see cref="IGenericAuthenticationProvider.CreateOrUpdateUser"/> behavior.
    /// </summary>
    [Flags]
    public enum UCLMode
    {
        /// <summary>
        /// Not applicable.
        /// </summary>
        None = 0,

        /// <summary>
        /// Only new user registration must be created for the provider.
        /// </summary>
        CreateOnly = 1,

        /// <summary>
        /// Only existing user registration information must be updated.
        /// </summary>
        UpdateOnly = 2,

        /// <summary>
        /// Creates or updates: this is often the default mode.
        /// </summary>
        CreateOrUpdate = 3,

        /// <summary>
        /// Consider the create or update as a "fake" login: login checks are
        /// done but no side effect (other than the potential update or create work)
        /// must occur.
        /// </summary>
        WithCheckLogin = 4,

        /// <summary>
        /// Consider the create or update as a login: checks are done and on success
        /// login side effects are triggered the LastLoginTime is updated.
        /// </summary>
        WithActualLogin = 8,

        /// <summary>
        /// Optimistic update requires a timestamp/version key.
        /// This flag enables implementation that support optimistic concurrency on updates 
        /// to handle the case where such key can not be provided (or is known to be invalid) 
        /// and hence, work as if no optimistic concurrency has been implemented.
        /// </summary>
        IgnoreOptimisticKey = 16
    }
}
