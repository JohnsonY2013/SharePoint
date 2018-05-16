using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hpe.utilities
{
    public static partial class HPEExtensions
    {
        public static bool Contains(this string[] array, string value, StringComparison comparisonType = StringComparison.InvariantCulture)
        {
            if (array == null || array.Length == 0) return false;

            foreach (var str in array)
            {
                if (str.Equals(value, comparisonType))
                {
                    return true;
                }
            }

            return false;
        }

        public static string[] Add(this string[] array, string value)
        {
            if (array == null) array = new string[] { };

            var arrayList = new List<string>();

            foreach (var item in array)
            {
                arrayList.Add(item);
            }

            arrayList.Add(value);

            return arrayList.ToArray();
        }

        public static string[] Concat(this string[] array, string[] array2)
        {
            if (array == null) array = new string[] { };
            if (array2 == null) array2 = new string[] { };

            var arrayList = new List<string>();

            foreach (var item in array)
            {
                arrayList.Add(item);
            }

            foreach (var item in array2)
            {
                arrayList.Add(item);
            }

            return arrayList.ToArray();
        }
    }
}
