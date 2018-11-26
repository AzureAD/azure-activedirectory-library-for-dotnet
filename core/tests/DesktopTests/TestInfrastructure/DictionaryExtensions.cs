using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Identity.AutomationTests
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Convert a non-generic <see cref="IDictionary"/> instance to the generic <see cref="IDictionary{TKey,TValue}"/>
        /// by calling <see cref="object.ToString"/> on both keys and values.
        /// </summary>
        /// <param name="dictionary">Non-generic dictionary of <see cref="object"/> key/value pairs</param>
        /// <returns>Generic <see cref="IDictionary{TKey,TValue}"/> of string/string key/value pairs</returns>
        public static IDictionary<string, string> ToStringDictionary(this IDictionary dictionary)
        {
            var genericDictionary = new Dictionary<string, string>(dictionary.Count);

            IDictionaryEnumerator enumerator = dictionary.GetEnumerator();
            while (enumerator.MoveNext())
            {
                genericDictionary.Add(enumerator.Key.ToString(), GetObjectAsString(enumerator.Value));
            }

            return genericDictionary;
        }

        public static IDictionary<TKey, string> ToStringDictionary<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary)
        {
            return dictionary.ToDictionary(x => x.Key, x => GetObjectAsString(x.Value));
        }

        /// <summary>
        /// Converts the object to a string, preserving the timezone in the case of date-times
        /// </summary>
        private static string GetObjectAsString(object obj)
        {
            string val;
            if (obj is DateTime)
            {
                // For date-times, preserve the full time zone information
                val = ((DateTime)obj).ToString("O");
            }
            else
            {
                val = obj.ToString();
            }
            return val;
        }

        /// <summary>
        /// Merges the dictionary into the current one. Values will be overriden for equal keys. Equality is defined by the current dictionary.
        /// </summary>
        public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> baseDictionary, IDictionary<TKey, TValue> mergeDictionary)
        {
            if (mergeDictionary != null)
            {
                foreach (var item in mergeDictionary)
                {
                    baseDictionary[item.Key] = item.Value;
                }
            }
        }
    }
}
