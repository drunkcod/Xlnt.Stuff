using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Xlnt.Data
{
    public static class IDataReaderExtensions
    {
        public static IEnumerable<T> As<T>(this IDataReader self, Converter<IDataReader, T> convert) {
            while(self.Read())
                yield return convert(self);
        }

        public static IEnumerable<T> As<T>(this IDataReader self) where T : new() {
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
            var fieldOrdinals = new int[fields.Length];
            for(var i = 0; i != fields.Length; ++i)
                fieldOrdinals[i] = self.GetOrdinal(fields[i].Name);
            return self.As<T>(reader => {
                var item = new T();
                for(var i = 0; i != fields.Length; ++i)
                    fields[i].SetValue(item, reader.GetValue(fieldOrdinals[i]));
                return item;
            });
        }
    }
}
