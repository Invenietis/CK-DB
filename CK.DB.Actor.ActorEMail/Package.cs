﻿using CK.Setup;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Actor.ActorEMail
{
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    [Versions( "1.0.0" )]
    public class Package : SqlPackage
    {
        void Construct( Actor.Package actorPackage )
        {
        }

    }
}