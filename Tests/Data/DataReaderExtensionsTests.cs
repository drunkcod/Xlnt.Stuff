using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xlnt.Data;
using Xlnt.Stuff;
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

        class WrappedValue<T>
        {
            public T Value { get; set; }

            public override string ToString() {
                return Value.ToString();
            }

            public override bool Equals(object obj) {
                return ((WrappedValue<T>)obj).Value.Equals(Value);
            }

            public override int GetHashCode() {
                return Value.GetHashCode();
            }
        }

        static Item<T> MakeItem<T>(T value){ return new Item<T>{ Value = value }; }
        static WrappedValue<T> MakeValue<T>(T value) { return new WrappedValue<T> { Value = value }; }

        [Test]
        public void As_supports_automatic_field_matching() {
            var items = new[] { MakeItem("SomeValue") };
            var data = items.AsDataReader();

            data.ColumnMappings.Add(x => x.Value);
            data.ColumnMappings.Add("Id", x => 1);
            Assert.That(
                data.As<Row>().ToList(),
                Is.EqualTo(new[]{ new Row{ Id = 1, Value = items[0].Value } }));        
        }
        [Test]
        public void As_ignores_missing_fields() {
            var items = new[] { MakeItem("SomeValue") };
            var data = items.AsDataReader();

            data.ColumnMappings.Add(x => x.Value);
            Assert.That(
                data.As<Row>().ToList(),
                Is.EqualTo(new[] { new Row { Value = items[0].Value } }));
        }
        [Test]
        public void As_supports_writable_properties() {
            var items = new[] { MakeValue("SomeValue") };
            var data = items.AsDataReader();

            data.ColumnMappings.Add(x => x.Value);
            Assert.That(
                data.As<WrappedValue<string>>().ToList(),
                Is.EqualTo(items));
        }

        class WithReadOnlyProperty : WrappedValue<int>
        {
            readonly int id;
            public WithReadOnlyProperty() : this(-1) { }
            public WithReadOnlyProperty(int id) { this.id = id; }

            public int Id { get { return id; } }
        }

        [Test]
        public void As_ignores_read_only_properties() {
            var items = new[] { new WithReadOnlyProperty(1){ Value = 2 } };
            var data = items.AsDataReader();

            data.ColumnMappings.Add(x => x.Id);
            data.ColumnMappings.Add(x => x.Value);
            Assert.That(
                data.As<WithReadOnlyProperty>().ToList(),
                Is.EqualTo(new[]{ new WithReadOnlyProperty{ Value = 2 } }));
        }

        [Test]
        public void Int32_roundtrip(){
            CheckRoundtrip(reader => MakeItem(reader.GetInt32(0)), MakeItem(42));
        }
        [Test]
        public void String_roundtrip(){
            CheckRoundtrip(reader => MakeItem(reader.GetString(0)), MakeItem("Hello World!"));
        }

        static void CheckRoundtrip<T>(Converter<IDataReader, Item<T>> toItem, params Item<T>[] items) {
            var data = items.AsDataReader();
            data.ColumnMappings.Add(x => x.Value);
            Assert.That(
                data.As<Item<T>>(toItem).ToList(),
                Is.EqualTo(items));
        }
    }
}