using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    public static class CollectionUtils
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection switch
            {
                ICollection        <T> c => c.Count == 0,
                IReadOnlyCollection<T> c => c.Count == 0,
                null => true,
                _    => collection.Any() == false
            };
        }
    }
}
