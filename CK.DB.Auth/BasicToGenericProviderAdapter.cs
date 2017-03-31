using CK.SqlServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CK.DB.Auth
{
    class BasicToGenericProviderAdapter : IGenericAuthenticationProvider
    {
        readonly IBasicAuthenticationProvider _basic;
        static public readonly string Name = "Basic";

        public BasicToGenericProviderAdapter(IBasicAuthenticationProvider basic)
        {
            Debug.Assert(basic != null);
            _basic = basic;
        }

        string IGenericAuthenticationProvider.ProviderName => Name;

        CreateOrUpdateResult IGenericAuthenticationProvider.CreateOrUpdateUser(ISqlCallContext ctx, int actorId, int userId, object payload, CreateOrUpdateMode mode)
        {
            string password = payload as string;
            if (password == null) throw new ArgumentException(nameof(payload));
            return _basic.CreateOrUpdatePasswordUser(ctx, actorId, userId, password, mode);
        }

        Task<CreateOrUpdateResult> IGenericAuthenticationProvider.CreateOrUpdateUserAsync(ISqlCallContext ctx, int actorId, int userId, object payload, CreateOrUpdateMode mode, CancellationToken cancellationToken)
        {
            string password = payload as string;
            if (password == null) throw new ArgumentException(nameof(payload));
            return _basic.CreateOrUpdatePasswordUserAsync(ctx, actorId, userId, password, mode, cancellationToken);
        }

        void IGenericAuthenticationProvider.DestroyUser(ISqlCallContext ctx, int actorId, int userId)
        {
            _basic.DestroyPasswordUser(ctx, actorId, userId);
        }

        Task IGenericAuthenticationProvider.DestroyUserAsync(ISqlCallContext ctx, int actorId, int userId, CancellationToken cancellationToken)
        {
            return _basic.DestroyPasswordUserAsync(ctx, actorId, userId, cancellationToken);
        }

        int? IGenericAuthenticationProvider.LoginUser(ISqlCallContext ctx, object payload, bool actualLogin)
        {
            Tuple<string, string> byName = payload as Tuple<string, string>;
            if (byName != null) return _basic.LoginUser(ctx, byName.Item1, byName.Item2, actualLogin);
            Tuple<int, string> byId = payload as Tuple<int, string>;
            if (byId != null) return _basic.LoginUser(ctx, byId.Item1, byId.Item2, actualLogin);
            return null;
        }

        async Task<int?> IGenericAuthenticationProvider.LoginUserAsync(ISqlCallContext ctx, object payload, bool actualLogin, CancellationToken cancellationToken)
        {
            Tuple<string, string> byName = payload as Tuple<string, string>;
            if (byName != null) return await _basic.LoginUserAsync(ctx, byName.Item1, byName.Item2, actualLogin, cancellationToken);
            Tuple<int, string> byId = payload as Tuple<int, string>;
            if (byId != null) return await _basic.LoginUserAsync(ctx, byId.Item1, byId.Item2, actualLogin);
            return null;
        }

    }
}
