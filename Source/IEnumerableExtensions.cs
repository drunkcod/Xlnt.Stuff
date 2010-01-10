using System;
using System.Collections.Generic;
using System.Data;
using Xlnt.Data;

namespace Xlnt.Stuff
{
    public static class IEnumerableExtensions
    {
        public static bool Any<T>(this IEnumerable<T> collection, Predicate<T> matches){
            foreach(var item in collection)
                if(matches(item))
                    return true;
            return false;
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> process){
            foreach(var item in collection)
                process(item);
        }

        public static bool Contains<T>(this IEnumerable<T> collection, T wanted){
            foreach (var item in collection)
                if (wanted.Equals(item))
                    return true;
            return false;
        }
    }
}
