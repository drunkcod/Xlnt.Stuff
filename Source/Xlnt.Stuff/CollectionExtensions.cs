﻿using System;
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

        public static bool Contains<T>(this IEnumerable<T> collection, T wanted) {
            foreach(var item in collection)
                if(wanted.Equals(item))
                    return true;
            return false;
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action){
            foreach(var item in collection)
                action(item);
        }

        public static void ForEach<T>(this T[] array, Action<T> action) {
            Array.ForEach(array, action);
        }
    }
}