using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Xlnt.Stuff;
using DataReaderColumn = System.Action<object, System.Data.IDataReader>;

namespace Xlnt.Data
{
    public static class IDataReaderExtensions
    {
        public static IEnumerable<T> As<T>(this IDataReader self, Converter<IDataReader, T> convert) {
            while(self.Read())
                yield return convert(self);
        }

        public static IEnumerable<T> As<T>(this IDataReader self) where T : new() {
            var knownFields = GetFieldOrdinals(self);
            var fields = new List<DataReaderColumn>();
            fields.AddRange(MatchFields(knownFields, typeof(T)));
            fields.AddRange(MatchProperties(knownFields, typeof(T)));
            return self.As<T>(reader => {
                var item = new T();
                fields.ForEach(x => x(item, reader));
                return item;
            });
        }   

        class CaseInsensitiveStringComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y) {
                return string.Compare(x, y, true) == 0;
            }

            public int GetHashCode(string obj) {
                return obj.ToLowerInvariant().GetHashCode();
            }
        }

        static Dictionary<string, int> GetFieldOrdinals(IDataReader reader){
            var knownFields = new Dictionary<string, int>(new CaseInsensitiveStringComparer());
            for(var i = 0; i != reader.FieldCount; ++i)
                knownFields.Add(reader.GetName(i), i);
            return knownFields;
        }

        static IEnumerable<DataReaderColumn> MatchFields(Dictionary<string,int> knownFields, Type type) {
            var allFields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            int ordinal;
            foreach(var item in allFields)
                if(knownFields.TryGetValue(item.Name, out ordinal))
                    yield return ColumnFrom(item, ordinal);
        }

        static IEnumerable<DataReaderColumn> MatchProperties(Dictionary<string, int> knownFields, Type type) {
            var allProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            int ordinal;
            foreach(var item in allProperties)
                if(item.CanWrite && knownFields.TryGetValue(item.Name, out ordinal))
                    yield return ColumnFrom(item, ordinal);
        }

        static DataReaderColumn ColumnFrom(FieldInfo field, int ordinal) {
            return (obj, reader) => field.SetValue(obj, reader.GetValue(ordinal));
        }

        static DataReaderColumn ColumnFrom(PropertyInfo property, int ordinal) {
            return (obj, reader) => property.SetValue(obj, reader.GetValue(ordinal), null);
        }
    }
}
