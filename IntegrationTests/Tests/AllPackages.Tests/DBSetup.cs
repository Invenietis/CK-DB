using CK.Core;
using CK.Monitoring;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPackages.Tests
{
    [TestFixture]
    public class DBSetup : CK.DB.Tests.DBSetup
    {
        static bool _grandOutputOpened;

        [Explicit]
        [Test]
        public void open_GrandOutput_Default()
        {
            if( !_grandOutputOpened )
            {
                _grandOutputOpened = true;
                string path = System.IO.Path.Combine( TestHelper.LogFolder, "BinLogs" );
                var c = new GrandOutputConfiguration();
                c.AddHandler( new CK.Monitoring.Handlers.BinaryFileConfiguration() { Path = path } );
                GrandOutput.EnsureActiveDefault( c );
                ActivityMonitor.DefaultFilter = LogFilter.Debug;
            }
        }

        [Explicit]
        [Test]
        public void dispose_GrandOutput_Default()
        {
            if( _grandOutputOpened )
            {
                GrandOutput.Default.Dispose();
                _grandOutputOpened = false;
            }
        }
    }
}
