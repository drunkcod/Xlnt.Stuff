using System;
using NUnit.Framework;

namespace Xlnt.Stuff
{
    public class TypeExtensionsTests
    {
        class MyClass
        {
            public const int DefaultValue = 42;

            readonly int value;

            public MyClass(): this(MyClass.DefaultValue){}
            public MyClass(int value) { this.value = value; }

            public int Value { get { return value; } }
        }

        [Test]
        public void ConstructAs_should_use_default_ctor_when_no_parameters_given() {
            var obj = typeof(MyClass).ConstructAs<MyClass>();
            Assert.That(obj.Value, Is.EqualTo(MyClass.DefaultValue));
        }
        [Test]
        public void ConstructAs_should_find_parameterized_ctor() {
            var value = MyClass.DefaultValue + 1;
            var obj = typeof(MyClass).ConstructAs<MyClass>(value);
            Assert.That(obj.Value, Is.EqualTo(value));
        
        }

    }
}