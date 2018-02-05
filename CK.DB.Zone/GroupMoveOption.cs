using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Zone
{
    /// <summary>
    /// Defines the behavior for existing users whenever a Group is 
    /// moved into a Zone.
    /// </summary>
    public enum GroupMoveOption
    {
        /// <summary>
        /// Group's users must be registered in the target Zone otherwise
        /// an exception is thrown.
        /// This is the safest option.
        /// </summary>
        None = 0,
        /// <summary>
        /// Only users that are already registered in the target Zone
        /// will be kept. Others will be automatically removed from the moved Groups.
        /// </summary>
        Intersect = 1,
        /// <summary>
        /// Users that are not registered in the target Zone will be automatically
        /// registered.
        /// </summary>
        AutoUserRegistration = 2
    }
}
