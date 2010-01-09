using System;

namespace Xlnt.Stuff
{
    public static class TypeExtensions
    {
        public static T ConstructAs<T>(this Type self, params object[] args){
            if(args.Length == 0)
                return New<T>(self, Type.EmptyTypes, null);
        
            var argumentTypes = new Type[args.Length];
            for (var i = 0; i != args.Length; ++i)
                argumentTypes[i] = args[i].GetType();
            return New<T>(self, argumentTypes, args);
        }

        static T New<T>(Type self, Type[] types, object[] args){
            return (T)self.GetConstructor(types).Invoke(args);
        }
    }
}