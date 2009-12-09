using System;
using System.Collections.Generic;

namespace Xlnt.Stuff
{
    public static class ObjectExtensions
    {
        public static IEnumerable<object> Select<T>(this T obj, IEnumerable<Func<T, object>> funcs){
            foreach(var fun in funcs)
                yield return fun(obj);
        }

        public static IEnumerable<object> Select<T>(this T obj, params Func<T, object>[] funcs){
            return Select<T>(obj, (IEnumerable<Func<T, object>>)funcs);
        }
    }
}