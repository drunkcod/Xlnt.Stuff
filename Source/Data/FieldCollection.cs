﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xlnt.Stuff;

namespace Xlnt.Data
{
    public class FieldCollection<T>
    {
        struct Field
        {
            public string Name;
            public Func<T, object> Read;
        }

        readonly List<Field> fields = new List<Field>();

        public int Count { get { return fields.Count; } }

        public FieldCollection<T> Add<TAny>(Expression<Func<T, TAny>> column)
        {
            return Add(GetName(column.Body), Lambdas.Box(column.Compile()));
        }

        static string GetName(Expression expression) {
            return ((MemberExpression)expression).Member.Name;
        }

        //Since F# doesn't autobox return values this is here for convinence.
        public FieldCollection<T> Add<TAny>(string name, Func<T, TAny> read) {
            return Add(name, Lambdas.Box(read));
        }

        public FieldCollection<T> Add(string name, Func<T, object> read) {
            fields.Add(new Field {
                Name = name,
                Read = read
            });
            return this;
        }

        public string GetName(int i) { return fields[i].Name; }
        public Func<T, object> GetReader(int i) { return fields[i].Read; }
        public object Read(T item, int i) { return GetReader(i)(item); }
    }
}