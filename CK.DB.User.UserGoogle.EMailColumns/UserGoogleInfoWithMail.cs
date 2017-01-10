using CK.DB.User.UserGoogle;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.User.UserGoogle.EMailColumns
{

    public class UserGoogleInfoWithMail : UserGoogleInfo, IUserGoogleInfoWithMail
    {
        public string EMail { get; set; }

        public bool? EMailVerified { get; set; }
    }
}
