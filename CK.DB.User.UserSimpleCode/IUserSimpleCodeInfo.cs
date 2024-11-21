using CK.Core;

namespace CK.DB.User.UserSimpleCode;

/// <summary>
/// Holds information stored for a SimpleCode user.
/// </summary>
public interface IUserSimpleCodeInfo : IPoco
{
    /// <summary>
    /// Gets or sets the SimpleCode.
    /// </summary>
    string SimpleCode { get; set; }
}
