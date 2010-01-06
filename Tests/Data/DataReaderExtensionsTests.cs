using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xlnt.Data;
using System.Data;
using System.Reflection;

namespace Xlnt.Tests.Data
{
    public class DataReaderExtensionsTests
    {
        class Item<T>
        {
            public T Value;

            public override string ToString() {
                return Value.ToString();
            }

            public override bool Equals(object obj) {
                return ((Item<T>)obj).Value.Equals(Value);
            }

            public override int GetHashCode() {
                return Value.GetHashCode();
            }
        }

        static Item<T> MakeItem<T>(T value){ return new Item<T>{ Value = value }; }

        [Test]
        public void As_supports_automatic_field_matching() {
            var items = new[] { MakeItem("SomeValue") };

            var data = new CollectionDataReader<Item<string>>(items);
            data.ColumnMappings.Add(x => x.Value);
            data.ColumnMappings.Add("Id", x => 1);
            Assert.That(
                data.As<Row>().ToList(),
                Is.EqualTo(new[]{ new Row{ Id = 1, Value = items[0].Value } }));        
        }
        [Test]
        public void As_ignores_missing_fields() {
            var items = new[] { MakeItem("SomeValue") };

            var data = new CollectionDataReader<Item<string>>(items);
            data.ColumnMappings.Add(x => x.Value);
            Assert.That(
                data.As<Row>().ToList(),
                Is.EqualTo(new[] { new Row { Value = items[0].Value } }));
        }

        [Test]
        public void Int32_roundtrip(){
            CheckRoundtrip(reader => MakeItem(reader.GetInt32(0)), MakeItem(42));
        }

        static void CheckRoundtrip<T>(Converter<IDataReader, Item<T>> toItem, params Item<T>[] items) {
            var data = new CollectionDataReader<Item<T>>(items);
            data.ColumnMappings.Add(x => x.Value);
            Assert.That(
                data.As<Item<T>>(toItem).ToList(),
                Is.EqualTo(items));
        }
    }
}