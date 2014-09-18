using System;
using System.Text;
using Cone;
using Xlnt.Data.Mapping;

namespace Xlnt.Data
{
	namespace Mapping 
	{
		class TableAttribute : Attribute
		{
			public string Name { get; set; }
		}

		class ColumnAttribute : Attribute
		{
			public string Name { get; set; }
		}
	}

	[Describe(typeof(XmlParameter<>))]
	public class XmlParameterSpec
	{
		[Table(Name = "#SingleColumnTable")]
		class SingleFieldColumnTable<T>
		{
			[Column]
			public T Value;
		}

		public void single_field_column_formatting() {
			var parameter = new XmlParameter<SingleFieldColumnTable<int>>("?");
			parameter.Add(new SingleFieldColumnTable<int> { Value = 42 });
			Check.That(() =>  parameter.GetValue() == "<p>42</p>");
		}

		[Table(Name = "#SingleColumnTable")]
		class SinglePropertyColumnTable<T>
		{
			[Column]
			public T Value { get; set; }
		}

		public void single_property_column_formatting() {
			var parameter = new XmlParameter<SinglePropertyColumnTable<int>>("?");
			parameter.Add(new SinglePropertyColumnTable<int> { Value = 42 });
			Check.That(() =>  parameter.GetValue() == "<p>42</p>");
		}

		public void xml_encodes_strings() {
			var parameter = new XmlParameter<SingleFieldColumnTable<string>>("?");
			parameter.Add(new SingleFieldColumnTable<string> { Value = "<>" });
			parameter.Add(new SingleFieldColumnTable<string> { Value = "!!" });
			Check.That(() =>  parameter.GetValue() == "<p>&lt;&gt;</p><p>!!</p>");
		}

	}
}
