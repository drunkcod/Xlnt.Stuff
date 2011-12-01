using System;
using System.Text;
using Cone;

namespace Xlnt.Data
{
    class TableAttribute : Attribute
    {
        public string Name { get; set; }
    }

    class ColumnAttribute : Attribute
    {
        public string Name { get; set; }
    }

    [Describe(typeof(XmlParameter<>))]
    public class XmlParameterSpec
    {
        [Table(Name = "#SingleColumnTable")]
        class SingleFieldColumnTable 
        {
            [Column]
            public int Value;
        }

        public void single_field_column_formatting() {
            var format = XmlParameter.GetFormatter(typeof(SingleFieldColumnTable));
            var result = new StringBuilder();
            format.Append(result, new SingleFieldColumnTable { Value = 42 });
            Verify.That(() =>  result.ToString() == "<p>42</p>");
        }

        [Table(Name = "#SingleColumnTable")]
        class SinglePropertyColumnTable 
        {
            [Column]
            public int Value { get; set; }
        }

        public void single_property_column_formatting() {
            var format = XmlParameter.GetFormatter(typeof(SinglePropertyColumnTable));
            var result = new StringBuilder();
            format.Append(result, new SinglePropertyColumnTable { Value = 42 });
            Verify.That(() =>  result.ToString() == "<p>42</p>");

        }

    }
}
