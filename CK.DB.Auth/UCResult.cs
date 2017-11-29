using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Auth
{
    /// <summary>
    /// Captures creation or update result.
    /// </summary>
    public enum UCResult
    {
        /// <summary>
        /// Nothing happened: <see cref="IGenericAuthenticationProvider.CreateOrUpdateUser"/> was called
        /// with <see cref="UCLMode.CreateOnly"/> and the user is already registered for the provider 
        /// OR with <see cref="UCLMode.UpdateOnly"/> and the user is not yet registered.
        /// </summary>
        None = 0,

        /// <summary>
        /// The user has been registered for the first time.
        /// </summary>
        Created = 1,

        /// <summary>
        /// Existing user registration information have been updated.
        /// </summary>
        Updated = 2,

        /// <summary>
        /// Whenever optimistic concurrency is supported
        /// and a concurrency error has been detected.
        /// </summary>
        OptimisticConcurrencyFailure = 3
    }
}
