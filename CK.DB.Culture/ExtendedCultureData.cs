using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DB.Culture
{
    /// <summary>
    /// Immutable extended culture data.
    /// </summary>
    public class ExtendedCultureData
    {
        readonly CultureData[] _fallbacks;

        /// <summary>
        /// Initializes a new immutable <see cref="ExtendedCultureData"/> (fallbacks are internally copied).
        /// </summary>
        /// <param name="xlcid">The extended culture identifier.</param>
        /// <param name="fallbacks">
        /// The fallbacks: must not be null or empty and must start with the culture identifier
        /// that corresponds to the <paramref name="xlcid"/>.
        /// </param>
        public ExtendedCultureData( int xlcid, IEnumerable<CultureData> fallbacks )
        {
            if( xlcid <= 0 || xlcid == 0xFFFF ) throw new ArgumentException( "Must be greater than 0 and not 0xFFFF.", nameof( xlcid ) );
            if( fallbacks == null ) throw new ArgumentNullException( nameof( fallbacks ) );
            XLCID = xlcid;
            _fallbacks = fallbacks.ToArray();
            if( _fallbacks.Length == 0 || _fallbacks[0].LCID != (xlcid & 0xFFFF) )
            {
                throw new ArgumentException( $"Invalid PrimaryCulture {_fallbacks[0].LCID}. Must be {xlcid&0xFFFF}.", nameof( xlcid ) );
            }
        }

        internal ExtendedCultureData( int xlcid, CultureData[] fallbacks )
        {
            XLCID = xlcid;
            _fallbacks = fallbacks;
        }

        /// <summary>
        /// Gets the extended culture identifier. Always greater than 0 and not equal to 0xFFFF. 
        /// </summary>
        public int XLCID { get; set; }

        /// <summary>
        /// Gets the primary culture: it is the first of the <see cref="Fallbacks"/>.
        /// </summary>
        public CultureData PrimaryCulture => _fallbacks[0];

        /// <summary>
        /// Gets the fallback list.
        /// </summary>
        public IReadOnlyList<CultureData> Fallbacks => _fallbacks;

    }
}
