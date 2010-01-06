using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Xlnt.Data
{
    public class FieldCollection<T>
    {
        struct Field
        {
            public string Name;
            public Func<T, string> GetValue;
        }

        readonly List<Field> fields = new List<Field>();

        public int Count { get { return fields.Count; } }

        public FieldCollection<T> Add<TAny>(Expression<Func<T, TAny>> column)
        {
            return Add(GetName(column), column.Compile());
        }

        static string GetName<TAny>(Expression<Func<T, TAny>> column) {
            return ((MemberExpression)column.Body).Member.Name;
        }

        public FieldCollection<T> Add<TAny>(string name, Func<T, TAny> column) {
            fields.Add(new Field {
                Name = name,
                GetValue = x => column(x).ToString()
            });
            return this;
        }

    }
}
