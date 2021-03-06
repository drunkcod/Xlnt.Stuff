﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Cone;
using Xlnt.Stuff;

namespace Xlnt.Data
{
	public class SqlBulkCopyFixture
	{
		const string ConnectionString = "Server=.;Integrated Security=SSPI";

		protected void WithConnection(Action<SqlConnection> action){
			using(var db = new SqlConnection(ConnectionString)) {
				db.Open();
				action(db);
			}
		}

		protected static SqlBulkCopy SqlBulkCopyForRows(SqlConnection db) {
			using(var command = db.CreateCommand()) {
				command.CommandText = "create table #rows(id int,value varchar(max))";
				command.ExecuteNonQuery();
			}
			return new SqlBulkCopy(db) { DestinationTableName = "#rows" };
		}

		protected static void CheckRows(SqlConnection db, params Row[] expected) {
			using(var command = db.CreateCommand()) {
				command.CommandText = "select id,value from #rows";
				var rows = new List<Row>();
				using(var reader = command.ExecuteReader())
					while(reader.Read())
						rows.Add(new Row { Id = reader.GetInt32(0), Value = reader.GetString(1) });
				Check.That(() => rows.Count == expected.Length);
				expected.ForEach((int n, Row row) => Check.That(() => row == rows[n]));
			}
		}
	}
}
