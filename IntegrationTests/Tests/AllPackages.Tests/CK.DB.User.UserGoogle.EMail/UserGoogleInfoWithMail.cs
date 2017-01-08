using CK.DB.User.UserGoogle;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.User.UserGoogle.EMail
{

    public class UserGoogleInfoWithMail : UserGoogleInfo
    {
        public string EMail { get; set; }

        public bool EMailVerified { get; set; }
    }
}
