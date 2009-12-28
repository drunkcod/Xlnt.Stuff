using System;
using System.Collections.Generic;

namespace Xlnt.Stuff
{
    public static class IEnumerableExtensions
    {
        public static void Each<T>(this IEnumerable<T> collection, Action<T> process)
        {
            foreach (var item in collection)
                process(item);
        }
    }
}
