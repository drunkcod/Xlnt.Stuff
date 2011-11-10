using System.Collections.Generic;

namespace Xlnt.Data
{
    public static class IEnumerableExtensions
    {
        public static CollectionDataReader<T> AsDataReader<T>(this IEnumerable<T> collection) {
            return new CollectionDataReader<T>(collection);
        }
    }
}
