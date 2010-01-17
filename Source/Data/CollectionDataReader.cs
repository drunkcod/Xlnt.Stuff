using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Xlnt.Stuff;

namespace Xlnt.Data
{
    public  class CollectionDataReader<T> : DataReaderBase
    {
        readonly IEnumerator<T> items;
        FieldCollection<T> columns = new FieldCollection<T>();

        public CollectionDataReader(IEnumerable<T> source) {
            items = source.GetEnumerator();
        }

        public FieldCollection<T> ColumnMappings { get { return columns; } }

        public override int FieldCount { get { return columns.Count; } }
        public override string GetName(int i) { return columns.GetName(i); }
        public override object GetValue(int i) { return columns.Read(items.Current, i); }
        public override bool Read() { return items.MoveNext(); }

        public void MapAll() {
            typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance).ForEach(field => ColumnMappings.Add(field.Name, x => field.GetValue(x)));
            typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).ForEach(property => ColumnMappings.Add(property.Name, x => property.GetValue(x, null)));
        }
    }
}