using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xlnt.Stuff;

namespace Xlnt.Data
{
    public struct Field<T>
    {
        public Field(string name, Func<T, object> read) {
            this.Name = name;
            this.Read = read;
        }

        public readonly string Name;
        public readonly Func<T, object> Read;
    }

    public class FieldCollection<T> : IEnumerable<Field<T>>
    {
        readonly List<Field<T>> fields = new List<Field<T>>();

        public int Count { get { return fields.Count; } }

        public FieldCollection<T> Add<TAny>(Expression<Func<T, TAny>> column) {
            return Add(GetName(column.Body), Lambdas.Box(column.Compile()));
        }

        static string GetName(Expression expression) {
            var memberExpression = expression as MemberExpression;
            if(memberExpression != null)
                return memberExpression.Member.Name;
            return ((MemberExpression)((UnaryExpression)expression).Operand).Member.Name;
        }

        //Since F# doesn't autobox return values this is here for convinence.
        public FieldCollection<T> Add<TAny>(string name, Func<T, TAny> read) {
            return Add(name, Lambdas.Box(read));
        }

        public FieldCollection<T> Add(string name, Func<T, object> read) {
            fields.Add(new Field<T>(name, read));
            return this;
        }

        public string GetName(int i) { return fields[i].Name; }
        public Func<T, object> GetReader(int i) { return fields[i].Read; }
        public object Read(T item, int i) { return GetReader(i)(item); }

        #region IEnumerable<T> Members

        IEnumerator<Field<T>> IEnumerable<Field<T>>.GetEnumerator() { return fields.GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return fields.GetEnumerator(); }

        #endregion
    }
}