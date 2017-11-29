using System;
using System.Collections.Generic;
using System.Text;

namespace CK.DB.Auth
{

    /// <summary>
    /// Encapsulates the result of a login.
    /// The stored procedure extension point CK.sAuthUserOnLogin can be used to centralize
    /// all the checks related to login acceptance or rejection.
    /// </summary>
    public struct LoginResult
    {
        readonly int _code;

        /// <summary>
        /// Initializes a new successful login with a positive user identifier.
        /// </summary>
        /// <param name="userId">The logged in user: must be greater than 0.</param>
        public LoginResult( int userId )
        {
            if( userId <= 0 ) throw new ArgumentException( "Must be positive.", nameof(userId) );
            _code = userId;
            FailureReason = null;
        }

        /// <summary>
        /// Initializes a new failed login with a non empty reason and an optional error code.
        /// </summary>
        /// <param name="failureReason">The reason of the failure. Must not be null or empty.</param>
        /// <param name="failureCode">Optional error code. Must be greater or equal to 1.</param>
        public LoginResult( string failureReason, int failureCode = 1 )
        {
            if( failureCode < 1 ) throw new ArgumentException( "Must be greater or equal to 1.", nameof( failureCode ) );
            if( String.IsNullOrWhiteSpace( failureReason ) ) throw new ArgumentException( "Must not be empty.", nameof( failureReason ) );
            _code = ~failureCode;
            FailureReason = failureReason;
        }

        /// <summary>
        /// Initializes a new failed login with a known error code.
        /// </summary>
        /// <param name="failureCode">Known error code.</param>
        public LoginResult( KnownLoginFailureCode failureCode )
        {
            if( failureCode == KnownLoginFailureCode.None )
            {
                _code = 0;
                FailureReason = null;
            }
            else
            {
                _code = ~(int)failureCode;
                FailureReason = failureCode.ToKnownString();
            }
        }

        /// <summary>
        /// Initializes a new login result. This is the constructor used by the database calls.
        /// If <paramref name="failureReason"/> is not empty or <see cref="failureCode"/> is not null,
        /// the <see cref="IsSuccessful"/> property is false and the <see cref="UserId"/> is
        /// automatically set to 0.
        /// Special case is when failureCode is zero or userId is zero and both failure reason
        /// and failure code are null: this result <see cref="IsEmpty"/>.
        /// </summary>
        /// <param name="userId">The user identifier. Must be greater or equal to 0.</param>
        /// <param name="failureReason">Reason can be null or empty.</param>
        /// <param name="failureCode">Optional error code.</param>
        public LoginResult( int userId, string failureReason, int? failureCode )
        {
            if( failureCode.HasValue )
            {
                if( failureCode.Value < 0 ) throw new ArgumentException( "Must be zero or positive.", nameof( failureCode ) );
                if( failureCode.Value == 0 )
                {
                    _code = 0;
                    FailureReason = null;
                    return;
                }
            }
            bool hasReason = !String.IsNullOrWhiteSpace( failureReason );
            if( hasReason || failureCode.HasValue )
            {
                _code = ~(failureCode ?? 1);
                FailureReason = hasReason
                                    ? failureReason
                                    : KnownLoginFailureCodeExtensions.ToKnownString( failureCode.Value );
            }
            else
            {
                if( userId < 0 ) throw new ArgumentException( "Must be zero or positive.", nameof( userId ) );
                _code = userId;
                FailureReason = null;
            }
        }

        /// <summary>
        /// Gets whether this result is empty: it has not been challenged.
        /// </summary>
        public bool IsEmpty => _code == 0;

        /// <summary>
        /// Gets the user identifier.
        /// Always 0 if login failed (<see cref="IsSuccessful"/> is false).
        /// </summary>
        public int UserId => _code > 0 ? _code : 0;

        /// <summary>
        /// Gets an optional error code.
        /// May be 0 even if <see cref="IsSuccessful"/> is false.
        /// </summary>
        public int FailureCode => _code < 0 ? ~_code : 0;

        /// <summary>
        /// Gets a reason for login failure.
        /// </summary>
        public string FailureReason { get; }

        /// <summary>
        /// Gets whether the login is successful: <see cref="UserId"/> is greater than 0.
        /// </summary>
        public bool IsSuccessful => UserId > 0;
    }
}
