using System;

namespace Xlnt
{
    public static class Lambdas
    {
        public static Func<T, object> Box<T, TAny>(Func<T, TAny> func) {
            return x => func(x);
        }
    }
}
