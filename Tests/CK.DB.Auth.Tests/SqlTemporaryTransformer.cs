using CK.Core;
using CK.SqlServer;
using CK.SqlServer.Parser;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace CK.DB.Auth.Tests
{
    static class TestHelper2
    {
        /// <summary>
        /// Applies a temporary transformation. The transformer must target an existing
        /// sql object that will be restored when the returned IDisposable.Dispose is closed. 
        /// </summary>
        /// <param name="this">This SqlDatabase.</param>
        /// <param name="transformer">Transformer text.</param>
        /// <returns>A disposable object that will restore the original object.</returns>
        public static IDisposable TemporaryTransform( this SqlDatabase @this, string transformer )
        {
            var parser = new SqlServerParser();
            var tResult = parser.ParseTransformer( transformer );
            if( tResult.IsError )
            {
                throw new ArgumentException( "Invalid transformation: " + tResult.ErrorMessage, nameof(transformer) );
            }
            ISqlServerTransformer t = tResult.Result;
            string targetName = t.TargetSchemaName;
            if( targetName == null )
            {
                throw new ArgumentException( "Transfomer must target a Sql object.", nameof( transformer ) );
            }
            string origin = @this.GetObjectDefinition( targetName );
            var oResult = parser.ParseObject( origin );
            if( oResult.IsError )
            {
                throw new Exception( "Unable to parse object definition: " + oResult.ErrorMessage );
            }
            ISqlServerObject o = oResult.Result;
            ISqlServerObject oT = t.SafeTransform( TestHelper.Monitor, o );
            if( oT == null )
            {
                throw new Exception( "Unable to apply transformer." );
            }
            string oType;
            switch( o.ObjectType )
            {
                case SqlServerObjectType.Procedure: oType = "procedure"; break;
                case SqlServerObjectType.View: oType = "view"; break;
                default: oType = "function"; break;
            }
            void Restore()
            {
                var safe = SqlHelper.SqlEncodeStringContent( o.SchemaName );
                @this.ExecuteNonQuery( $"if OBJECT_ID('{safe}') is not null drop {oType} {o.SchemaName};" );
                @this.ExecuteNonQuery( origin );
            }
            try
            {
                @this.ExecuteNonQuery( $"drop {oType} {o.SchemaName};" );
                @this.ExecuteNonQuery( oT.ToFullString() );
                return Util.CreateDisposableAction( Restore );
            }
            catch
            {
                Restore();
                throw;
            }
        }

    }


}
