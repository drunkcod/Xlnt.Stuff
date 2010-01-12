using System;

namespace Xlnt.Stuff
{
    public static class Lambdas
    {
        public static Func<T, object> Box<T, TAny>(Func<T, TAny> func) {
            return x => func(x);
        }

        public static Func<T> Lazy<T>(Func<T> func) {
            Func<T> forced = null;
            return () => {
                if(forced != null)
                    return forced();
                var value = func();
                forced = () => value;
                return value;
            };
        }
    }
}
