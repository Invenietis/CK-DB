using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CK.DB.Auth
{
    /// <summary>
    /// Helpers methods to convert authentication payload into typed POCO.
    /// </summary>
    static public class PocoFactotyExtensions
    {
        /// <summary>
        /// Extracts payload by first checking whether <paramref name="payload"/> is already a <typeparamref name="T"/>
        /// and then by trying <see cref="ExtractPayload{T}(IPocoFactory{T}, IEnumerable{KeyValuePair{string, object}})"/>.
        /// </summary>
        /// <typeparam name="T">The POCO type.</typeparam>
        /// <param name="this">This POCO factory.</param>
        /// <param name="payload">The payload. Must not be null.</param>
        /// <returns>The resulting POCO.</returns>
        public static T ExtractPayload<T>(this IPocoFactory<T> @this, object payload) where T : IPoco
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            if (payload is T) return (T)payload;
            var kindOfDic = payload as IEnumerable<KeyValuePair<string, object>>;
            if (kindOfDic != null) return ExtractPayload(@this, kindOfDic);
            throw new ArgumentException($"Invalid payload. It must be a '{typeof(T).Name}' POCO or a IEnumerable<KeyValuePair<string, object>>.", nameof(payload));
        }
        /// <summary>
        /// Populates a new instance of <typeparamref name="T"/> with the provided KeyValuePair&lt;string, object&gt;.
        /// </summary>
        /// <typeparam name="T">The POCO type.</typeparam>
        /// <param name="this">This POCO factory.</param>
        /// <param name="payload">The payload. Must not be null.</param>
        /// <returns>The resulting POCO.</returns>
        public static T ExtractPayload<T>(
            this IPocoFactory<T> @this, 
            IEnumerable<KeyValuePair<string, object>> payload) where T : IPoco
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            T info = @this.Create();
            var properties = @this.PocoClassType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var kv in payload)
            {
                var property = properties.FirstOrDefault(p => StringComparer.OrdinalIgnoreCase.Equals(kv.Key, p.Name));
                if (property != null)
                {
                    try
                    {
                        var targetType = property.PropertyType;
                        var pType = targetType.GetTypeInfo();
                        if (pType.IsGenericType && (pType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                        {
                            if (kv.Value == null)
                            {
                                property.SetValue(info, null);
                                continue;
                            }
                            targetType = Nullable.GetUnderlyingType(targetType);
                            pType = targetType.GetTypeInfo();
                        }
                        object value;
                        string stringValue;
                        if (pType.IsEnum && (stringValue = kv.Value as string) != null)
                        {
                            value = Enum.Parse(targetType, stringValue);
                        }
                        else value = Convert.ChangeType(kv.Value, targetType);
                        property.SetValue(info, value);
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException($"Invalid payload. Unable to set '{property.Name}'. See inner exceptions for details.", nameof(payload), ex);
                    }
                }
            }
            return info;
        }

    }
}
