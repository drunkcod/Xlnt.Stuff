using System.Collections.Generic;

namespace Xlnt.Stuff
{
    public static class ObjectExtensions
    {
        public static object[] Map<T>(this T obj, params Func<T, object>[] funcs){
            var values = new List<object>();
            foreach (var fun in funcs)
                values.Add(fun(obj));
            return values.ToArray();
        }
    }
}