using System;

namespace Xlnt.Stuff
{
    public static class Lambdas
    {
        public static Func<T, object> Box<T, TAny>(Func<T, TAny> func) {
            if(typeof(TAny) == typeof(object))
                return func as Func<T, object>;
            return x => func(x);
        }

        public static Func<T> Lazy<T>(Func<T> func) {
            T value = default(T);
            return () => {
                if(func == null)
                    return value;
                value = func();
                func = null;
                return value;
            };
        }
    }
}
