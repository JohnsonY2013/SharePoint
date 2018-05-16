using System.Collections.Generic;

namespace HP.Analytics.Service.Commons.Extensions
{
    public static class ExtIEnumerable
    {
        public static void AddTo<T>(this IEnumerable<T> self, List<T> destination)
        {
            if (self != null) destination.AddRange(self);
        }
    }

}