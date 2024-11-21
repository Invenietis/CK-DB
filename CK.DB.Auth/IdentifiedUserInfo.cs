using CK.Core;

namespace CK.DB.Auth;

/// <summary>
/// Associates a user identifier to a user information.
/// </summary>
public class IdentifiedUserInfo<T> where T : class, IPoco
{
    /// <summary>
    /// Initializes a new <see cref="IdentifiedUserInfo{T}"/>.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="info">The user information.</param>
    public IdentifiedUserInfo( int userId, T info )
    {
        UserId = userId;
        Info = info;
    }

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public int UserId { get; }

    /// <summary>
    /// Gets or sets the associated information.
    /// Since <typeparamref name="T"/> is a <see cref="IPoco"/>, this can be casted
    /// to any specialized info interface defined by other packages.
    /// </summary>
    public T Info { get; }
}
