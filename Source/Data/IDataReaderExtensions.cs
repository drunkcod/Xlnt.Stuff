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
            return self.As<T>(reader => {
                var item = new T();
                fields.ForEach(x => x.ReadValue(item, reader));
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

        struct DataReaderField
        {
            readonly FieldInfo field;
            readonly int ordinal;

            public DataReaderField(FieldInfo field, int ordinal) {
                this.field = field;
                this.ordinal = ordinal;
            }

            public void ReadValue(object obj, IDataReader reader) {
                field.SetValue(obj, reader.GetValue(ordinal));
            }
        }

        static List<DataReaderField> MatchFields(IDataReader reader, Type type) {
            var knownFields = new Dictionary<string, int>(new CaseInsensitiveStringComparer());
            for(var i = 0; i != reader.FieldCount; ++i)
                knownFields.Add(reader.GetName(i), i);

            var fields = new List<DataReaderField>();
            var allFields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            int ordinal;
            foreach(var field in allFields)
                if(knownFields.TryGetValue(field.Name, out ordinal))
                    fields.Add(new DataReaderField(field, ordinal));

            return fields;
        }
    }
}
