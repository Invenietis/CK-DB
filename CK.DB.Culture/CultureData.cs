﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Culture
{
    /// <summary>
    /// Immutable actual culture: the LCID is between 0 and 0xFFFF.
    /// </summary>
    public class CultureData
    {
        /// <summary>
        /// Initializes a new <see cref="CultureData"/>.
        /// </summary>
        /// <param name="lcid">The culture identifier. Must be between 0 and 0xFFFF.</param>
        /// <param name="name">The name ("fr-FR"). See <see cref="CheckNameArgument"/>.</param>
        /// <param name="englishName">The english name ("French"). See <see cref="CheckNameArgument"/>.</param>
        /// <param name="nativeName">The native name ("Français"). See <see cref="CheckNameArgument"/>.</param>
        public CultureData( int lcid, string name, string englishName, string nativeName )
        {
            if( lcid <= 0 || lcid >= 0xFFFF ) throw new ArgumentException( "Must be between 0 and 0xFFFF.", nameof( lcid ) );
            CheckNameArgument( name, nameof( name ) );
            CheckNameArgument( englishName, nameof( englishName ) );
            CheckNameArgument( nativeName, nameof( nativeName ) );
            LCID = lcid;
            Name = name;
            EnglishName = englishName;
            NativeName = nativeName;
        }

        internal static readonly char[] _separators = new[] { ',', '|' };

        /// <summary>
        /// Checks name validity: not empty and must not contain comma (,) or pipe (|).
        /// </summary>
        /// <param name="name">Name to check.</param>
        /// <param name="parameterName">Parameter name.</param>
        public static void CheckNameArgument( string name, string parameterName )
        {
            if( string.IsNullOrWhiteSpace( name ) || name.IndexOfAny( _separators ) >= 0 )
            {
                throw new ArgumentException( "Must not be null or empty nor contain , or |.", parameterName );
            }
        }

        /// <summary>
        /// Gets the culture identifier (greater than 0 and lower than 0xFFFF).
        /// </summary>
        public int LCID { get; }

        /// <summary>
        /// Gets the official name, ie. "en", "fr-BE".
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the english name, ie. "French".
        /// </summary>
        public string EnglishName { get; }

        /// <summary>
        /// Gets or sets the native name, ie. "Français".
        /// </summary>
        public string NativeName { get; }
    }
}