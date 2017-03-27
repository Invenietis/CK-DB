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
    /// Specialized <see cref="IGenericAuthenticationTableProvider"/> that implements storage.
    /// </summary>
    public interface IGenericAuthenticationTableProvider : IGenericAuthenticationProvider
    {
        /// <summary>
        /// Gets the name of the provider.
        /// This name is unique, must be the one added to the tAuthProvider table.
        /// </summary>
        string ProviderName { get; }

    }
}
