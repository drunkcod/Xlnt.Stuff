using Cone;

namespace Xlnt.Stuff
{
    [Describe(typeof(TypeExtensions))]
    public class TypeExtensionsTests
    {
        class MyClass
        {
            public const int DefaultValue = 42;

            readonly int value;

            public MyClass(): this(DefaultValue){}
            public MyClass(int value) { this.value = value; }

            public int Value { get { return value; } }
        }

        public void ConstructAs_uses_default_ctor_when_no_parameters_given() {
            var obj = typeof(MyClass).ConstructAs<MyClass>();
            Verify.That(() => obj.Value == MyClass.DefaultValue);
        }

        public void ConstructAs_finds_parameterized_ctor() {
            var value = MyClass.DefaultValue + 1;
            var obj = typeof(MyClass).ConstructAs<MyClass>(value);
            Verify.That(() => obj.Value == value);
        }
    }
}