using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Xlnt.Stuff;

namespace Xlnt.Data
{
    public static class IDataReaderExtensions
    {
        public static IEnumerable<T> As<T>(this IDataReader self, Converter<IDataReader, T> convert) {
            while(self.Read())
                yield return convert(self);
        }

        public static IEnumerable<T> As<T>(this IDataReader self) where T : new() {
            var fields = MatchFields(self, typeof(T));
            var properties = MatchProperties(self, typeof(T));
            return self.As<T>(reader => {
                var item = new T();
                fields.ForEach(x => x.ReadValue(item, reader));
                properties.ForEach(x => x.ReadValue(item, reader));
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

        struct DataReaderColumn
        {
            readonly Action<object, IDataReader> readValue;

            public static DataReaderColumn From(FieldInfo field, int ordinal){
                return new DataReaderColumn((obj, reader) => field.SetValue(obj, reader.GetValue(ordinal)));
            }

            public static DataReaderColumn From(PropertyInfo property, int ordinal) {
                return new DataReaderColumn((obj, reader) => property.SetValue(obj, reader.GetValue(ordinal), null));
            }

            DataReaderColumn(Action<object,IDataReader> readValue){
                this.readValue = readValue;
            }

            public void ReadValue(object obj, IDataReader reader) {
                readValue(obj, reader);
            }
        }

        static List<DataReaderColumn> MatchFields(IDataReader reader, Type type) {
            var knownFields = new Dictionary<string, int>(new CaseInsensitiveStringComparer());
            for(var i = 0; i != reader.FieldCount; ++i)
                knownFields.Add(reader.GetName(i), i);

            var fields = new List<DataReaderColumn>();
            var allFields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            int ordinal;
            foreach(var field in allFields)
                if(knownFields.TryGetValue(field.Name, out ordinal))
                    fields.Add(DataReaderColumn.From(field, ordinal));

            return fields;
        }

        static List<DataReaderColumn> MatchProperties(IDataReader reader, Type type) {
            var knownFields = new Dictionary<string, int>(new CaseInsensitiveStringComparer());
            for(var i = 0; i != reader.FieldCount; ++i)
                knownFields.Add(reader.GetName(i), i);

            var properties = new List<DataReaderColumn>();
            var allProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            int ordinal;
            foreach(var item in allProperties)
                if(item.CanWrite && knownFields.TryGetValue(item.Name, out ordinal))
                    properties.Add(DataReaderColumn.From(item, ordinal));

            return properties;
        }
    }
}
