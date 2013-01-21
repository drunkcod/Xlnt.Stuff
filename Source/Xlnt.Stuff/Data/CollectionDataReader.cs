using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Linq.Expressions;
using Xlnt.Stuff;

namespace Xlnt.Data
{
    public  class CollectionDataReader<T> : DataReaderBase
    {
        const BindingFlags MappedMembers = BindingFlags.Public | BindingFlags.Instance;
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
		public override bool IsDBNull(int i) {
			return GetValue(i) is DBNull;
		}

        public CollectionDataReader<T> MapAll() {
            typeof(T).GetFields(MappedMembers).ForEach(Map);
            typeof(T).GetProperties(MappedMembers).ForEach(Map);
			return this;
        }

        void Map(MemberInfo member) {
            ColumnMappings.Add(GetMember(member));
        }

        Expression<Func<T,object>> GetMember(MemberInfo member) {
            return LambdaBox(parameter => Expression.MakeMemberAccess(parameter, member));
        }

        Expression<Func<T, object>> LambdaBox(Func<ParameterExpression,MemberExpression> getMember) {
            var parameter = Expression.Parameter(typeof(T), "x");
            return Expression.Lambda<Func<T, object>>(
                Expression.MakeUnary(
                    ExpressionType.Convert,
                    getMember(parameter),
                    typeof(object)),
                parameter);
        }
    }
}