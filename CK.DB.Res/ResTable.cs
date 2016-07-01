using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Res
{
    [SqlTable( "tRes", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    [SqlObjectItem( "vRes" )]
    public abstract class ResTable : SqlTable
    {
        /// <summary>
        /// Creates a new resource identifier.
        /// </summary>
        /// <param name="ctx">The required call context.</param>
        /// <returns>The resource identifier.</returns>
        [SqlProcedure( "sResCreate" )]
        public abstract Task<int> CreateAsync( ISqlCallContext ctx );

        /// <summary>
        /// Destroys a resource.
        /// </summary>
        /// <param name="ctx">The required call context.</param>
        /// <param name="resId">The resource identifier.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sResDestroy" )]
        public abstract Task DestroyAsync( ISqlCallContext ctx, int resId );

        /// <summary>
        /// Creates a new resource identifier.
        /// </summary>
        /// <param name="ctx">The required call context.</param>
        /// <returns>The resource identifier.</returns>
        [SqlProcedure( "sResCreate" )]
        public abstract int Create( ISqlCallContext ctx );

        /// <summary>
        /// Destroys a resource.
        /// </summary>
        /// <param name="ctx">The required call context.</param>
        /// <param name="resId">The resource identifier.</param>
        [SqlProcedure( "sResDestroy" )]
        public abstract void Destroy( ISqlCallContext ctx, int resId );

    }
}
