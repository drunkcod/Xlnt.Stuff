using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.IO;
using System.Linq;
using Cone;
using Cone.Helpers;

namespace Xlnt.Data
{
	class InMemoryDbCommand : DbCommand 
	{
		readonly InMemoryDbConnection connection;

		public InMemoryDbCommand(InMemoryDbConnection connection) {
			this.connection = connection;
		}

		public override string CommandText { get; set; }

		protected override DbDataReader ExecuteDbDataReader(System.Data.CommandBehavior behavior) {
			return connection.OnExecuteReader(this);
		}

		public override object ExecuteScalar() {
			return connection.OnExecuteScalar(this);
		}

		public override int CommandTimeout { get; set; }

		protected override DbTransaction DbTransaction {
			get; set;
		}

		public override void Cancel()
		{
			throw new NotImplementedException();
		}


		public override System.Data.CommandType CommandType
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		protected override DbParameter CreateDbParameter()
		{
			throw new NotImplementedException();
		}

		protected override DbConnection DbConnection
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		protected override DbParameterCollection DbParameterCollection
		{
			get { throw new NotImplementedException(); }
		}

		public override bool DesignTimeVisible
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public override int ExecuteNonQuery()
		{
			throw new NotImplementedException();
		}

		public override void Prepare()
		{
			throw new NotImplementedException();
		}

		public override System.Data.UpdateRowSource UpdatedRowSource
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}
	}
	
	class InMemoryDbConnection : DbConnection
	{

		ConnectionState state = ConnectionState.Closed;

		public InMemoryDbConnection() {
			ConnectionString = string.Empty;
		}

		public Func<InMemoryDbCommand, DbDataReader> OnExecuteReader = _ => DbDataReaderAdapter.From(new object[0]);
		public Func<InMemoryDbCommand, object> OnExecuteScalar = _ => null;

		protected override DbCommand CreateDbCommand() {
			return new InMemoryDbCommand(this);
		}

		public override string ConnectionString { get; set; }

		public override ConnectionState State {
			get { return state; }
		}

		public override void Open() {
			state = ConnectionState.Open;
		}

		public override void Close() {
			state = ConnectionState.Closed;
		}

		public override string ServerVersion {
			get { return "0.0.0.0"; }
		}

		protected override DbTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel)
		{
			throw new NotImplementedException();
		}

		public override void ChangeDatabase(string databaseName)
		{
			throw new NotImplementedException();
		}

		public override string DataSource
		{
			get { throw new NotImplementedException(); }
		}

		public override string Database
		{
			get { throw new NotImplementedException(); }
		}
	}

	class DbDataReaderAdapter : DbDataReader
	{
		readonly IDataReader inner;

		DbDataReaderAdapter(IDataReader inner) {
			this.inner = inner;
		}

		public static DbDataReader From<T>(IEnumerable<T> rows) {
			return new DbDataReaderAdapter(rows.AsDataReader().MapAll());
		}

		public override void Close() { inner.Close(); }

		public override int Depth { get { return inner.Depth; } }

		public override int FieldCount { get { return inner.FieldCount; } }

		public override bool GetBoolean(int ordinal) { return inner.GetBoolean(ordinal); }

		public override byte GetByte(int ordinal) { return inner.GetByte(ordinal); }

		public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) {
			return inner.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
		}

		public override char GetChar(int ordinal) { return inner.GetChar(ordinal); }

		public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) {
			return inner.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
		}

		public override string GetDataTypeName(int ordinal) { return inner.GetDataTypeName(ordinal); }

		public override DateTime GetDateTime(int ordinal) { return inner.GetDateTime(ordinal); }

		public override decimal GetDecimal(int ordinal) { return inner.GetDecimal(ordinal); }

		public override double GetDouble(int ordinal) { return inner.GetDouble(ordinal); }

		public override Type GetFieldType(int ordinal) { return inner.GetFieldType(ordinal); }

		public override float GetFloat(int ordinal) { return inner.GetFloat(ordinal); }

		public override Guid GetGuid(int ordinal) { return inner.GetGuid(ordinal); }

		public override short GetInt16(int ordinal) { return inner.GetInt16(ordinal); }

		public override int GetInt32(int ordinal) { return inner.GetInt32(ordinal); }

		public override long GetInt64(int ordinal) { return inner.GetInt64(ordinal); }

		public override string GetName(int ordinal) { return inner.GetName(ordinal); }

		public override int GetOrdinal(string name) { return inner.GetOrdinal(name); }

		public override DataTable GetSchemaTable() { return inner.GetSchemaTable(); }

		public override string GetString(int ordinal) { return inner.GetString(ordinal); }

		public override object GetValue(int ordinal) { return inner.GetValue(ordinal); }

		public override int GetValues(object[] values) { return inner.GetValues(values); }

		public override bool IsClosed { get { return inner.IsClosed; } }

		public override bool IsDBNull(int ordinal) { return inner.IsDBNull(ordinal); }

		public override bool NextResult() { return inner.NextResult(); }

		public override bool Read() { return inner.Read(); }

		public override int RecordsAffected { get { return inner.RecordsAffected; } }

		public override object this[string name] { get { return inner[name]; } }

		public override object this[int ordinal] { get { return inner[ordinal]; } }

		public override System.Collections.IEnumerator GetEnumerator() {
			throw new NotSupportedException();
		}

		public override bool HasRows {
			get { throw new NotSupportedException(); }
		}
	}

	[Describe(typeof(DbProfilingSession))]
	public class DbProfilingSessionSpec
	{
		static string DataPath { get { return Path.GetDirectoryName(new Uri(typeof(DbProfilingSessionSpec).Assembly.CodeBase).LocalPath); } }

		static InMemoryDbConnection OpenSampleConnection() {
			return new InMemoryDbConnection();
		}

		public class TracingContext : ITestContext
		{
			readonly List<IDisposable> garbage = new List<IDisposable>();
			public TracingEventProfilingSessionListener Trace;
			public DbConnection Connection;
			
			public void Before() {
				Trace = new TracingEventProfilingSessionListener();
				Connection = new DbProfilingSession(Trace).Connect(OpenSampleConnection());
				garbage.Add(Connection);
			}

			public void After(ITestResult result) {
				garbage.ForEach(x => x.Dispose());
				garbage.Clear();
			}

			public DbDataReader ExecuteReader(string query) {
				var q = Connection.CreateCommand();
				q.CommandText = query;
				var reader = q.ExecuteReader();
				garbage.Add(reader);
				return reader;
			}
		}

		public TracingContext Context = new TracingContext();

		TracingEventProfilingSessionListener Trace { get { return Context.Trace; } }
		DbDataReader ExecuteReader(string query) { return Context.ExecuteReader(query); }

		public void BatchStarted_when_executing_reader() {
			var batchStarted = new EventSpy<QueryEventArgs>();
			Trace.BeginBatch += batchStarted;
			ExecuteReader("select * from Numbers");

			Check.That(() => batchStarted.HasBeenCalled);
		}

		public void batch_starts_before_query() {
			var batchStarted = new EventSpy<QueryEventArgs>();
			var queryStarted = new EventSpy<QueryEventArgs>();
			Trace.BeginBatch += batchStarted;
			Trace.BeginQuery += queryStarted;
			ExecuteReader("select * from Numbers");

			Check.That(() => batchStarted.CalledBefore(queryStarted));
		}

		public void EndBatch_when_reader_closed() {
			var batchEnded = new EventSpy<QueryEventArgs>();
			Trace.EndBatch += batchEnded;
		   
			var reader = ExecuteReader("select * from Numbers");
			Check.That(() => !batchEnded.HasBeenCalled);
			reader.Close();
			Check.That(() => batchEnded.HasBeenCalled);
		}

		[DisplayAs("Ado.Net usage")]
		public void basic_usage() {
			var session = new DbProfilingSession();
			var db = session.Connect(OpenSampleConnection());

			var query = db.CreateCommand();
			query.CommandText = "select sum(value) from Numbers";
			query.ExecuteScalar();

			Check.That(
				() => session.QueryCount == 1,
				() => session.RowCount == 0);
		}

		[Context("Linq2Sql usage")]
		public class Linq2Sql 
		{
			//Sample Table.
			[Table(Name = "Numbers")]
			public class Number
			{
				[Column(Name = "Value")]
				public int Value;    
			}

			readonly List<Number> Numbers = new List<Number>
			{
				new Number { Value = 1 },
				new Number { Value = 2 },
				new Number { Value = 3 },
			};

			int NumbersRowCount { 
				get { return Numbers.Count; }
			}

			public void compare_deferred_and_local_execution() {
				var session = new DbProfilingSession();
				var db = OpenSampleConnection();
				var context = new DataContext(session.Connect(db));
				var numbers = context.GetTable<Number>(); 

				db.OnExecuteReader = command => {
					if(command.CommandText == "SELECT SUM([t0].[Value]) AS [value]\r\nFROM [Numbers] AS [t0]") {
						return DbDataReaderAdapter.From(new[]{ new { value = 42 } });
					} else if(command.CommandText == "SELECT [t0].[Value]\r\nFROM [Numbers] AS [t0]") {
						return DbDataReaderAdapter.From(Numbers);
					}
					throw new InvalidAssumptionException("Unexpected query = " + command.CommandText, null);
				};
				var deffered = session.Scoped("Sent to database for execution", _ => {
					numbers.Sum(x => x.Value);
				});                    

				var inMemory = session.Scoped("Pull all rows to memory", _ => {
					numbers.AsEnumerable().Sum(x => x.Value);                   
				});

				Check.That(
					() => deffered.QueryCount == 1,
					() => deffered.RowCount == 1,
					
					() => inMemory.QueryCount == 1,
					() => inMemory.RowCount == NumbersRowCount);

				Check.That(
					() => session.QueryCount == deffered.QueryCount + inMemory.QueryCount,
					() => session.RowCount == deffered.RowCount + inMemory.RowCount);
			}
		}
	}
}
