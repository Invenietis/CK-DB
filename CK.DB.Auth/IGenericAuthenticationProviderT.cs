using CK.Core;
using CK.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CK.DB.Auth
{

    /// <summary>
    /// Specializes <see cref="IGenericAuthenticationProvider"/> to expose its typed
    /// payload.
    /// </summary>
    /// <typeparam name="T">Type of the payload.</typeparam>
    public interface IGenericAuthenticationProvider<T> : IGenericAuthenticationProvider where T : class
    {
        /// <summary>
        /// Creates an empty payload object.
        /// </summary>
        /// <returns>A payload object.</returns>
        T CreatePayload();
    }
}
