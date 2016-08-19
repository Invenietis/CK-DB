﻿using CK.Setup;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Res.ResName
{
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    public class Package : SqlPackage
    {
        void Construct( Res.Package resource )
        {
        }

        [InjectContract]
        public ResTable ResTable { get; protected set; }

        [InjectContract]
        public ResNameTable ResNameTable { get; protected set; }
    }
}