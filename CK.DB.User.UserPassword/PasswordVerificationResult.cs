using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.User.UserPassword
{
    enum PasswordVerificationResult
    {
        Failed,
        SuccessRehashNeeded,
        Success
    }
}
