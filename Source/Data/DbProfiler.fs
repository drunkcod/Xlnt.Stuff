namespace Xlnt.Data

open System
open System.Diagnostics
open System.Data
open System.Data.Common
open System.Threading

module Timed =
    let action f report =
        let stopwatch = Stopwatch.StartNew()
        let r = f()
        stopwatch.Stop()
        report(stopwatch.Elapsed)
        r

type ProfiledDataReader(inner:DbDataReader) = 
    inherit DbDataReader() with
        let beginRow = new Event<_>()
        let endRow = new Event<_>()

        override this.Depth
            with get() = inner.Depth

        override this.FieldCount
            with get() = inner.FieldCount

        override this.HasRows
            with get() = inner.HasRows

        override this.IsClosed
            with get() = inner.IsClosed

        override this.RecordsAffected 
            with get() = inner.RecordsAffected

        override this.Item
            with get(ordinal:int) = inner.[ordinal]

        override this.Item
            with get(name:string) = inner.[name]

        override this.Close() = inner.Close()

        override this.GetDataTypeName(ordinal) = inner.GetDataTypeName(ordinal)

        override this.GetEnumerator() = inner.GetEnumerator()

        override this.GetFieldType(ordinal) = inner.GetFieldType(ordinal)

        override this.GetName(ordinal) = inner.GetName(ordinal)

        override this.GetOrdinal(name) = inner.GetOrdinal(name)

        override this.GetSchemaTable() = inner.GetSchemaTable()

        override this.GetBoolean(ordinal) = inner.GetBoolean(ordinal)

        override this.GetByte(ordinal) = inner.GetByte(ordinal)

        override this.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length) = inner.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length)

        override this.GetChar(ordinal) = inner.GetChar(ordinal)

        override this.GetChars(ordinal, dataOffset, buffer, bufferOffset, length) = inner.GetChars(ordinal, dataOffset, buffer, bufferOffset, length)

        override this.GetDateTime(ordinal) = inner.GetDateTime(ordinal)

        override this.GetDecimal(ordinal) = inner.GetDecimal(ordinal)

        override this.GetDouble(ordinal) = inner.GetDouble(ordinal)

        override this.GetFloat(ordinal) = inner.GetFloat(ordinal)

        override this.GetGuid(ordinal) = inner.GetGuid(ordinal)

        override this.GetInt16(ordinal) = inner.GetInt16(ordinal)

        override this.GetInt32(ordinal) = inner.GetInt32(ordinal)

        override this.GetInt64(ordinal) = inner.GetInt64(ordinal)

        override this.GetString(ordinal) = inner.GetString(ordinal)

        override this.GetValue(ordinal) = inner.GetValue(ordinal)

        override this.GetValues(values) = inner.GetValues(values)

        override this.IsDBNull(ordinal) = inner.IsDBNull(ordinal)

        override this.NextResult() = inner.NextResult()

        override this.Read() =
            beginRow.Trigger(this)            
            Timed.action inner.Read (fun elapsed -> endRow.Trigger((this, elapsed)))

        interface IDisposable with
            member this.Dispose() = 
                inner.Dispose()
                base.Dispose()

        member this.BeginRow = beginRow.Publish
        member this.EndRow = endRow.Publish

type ProfiledCommand(inner:DbCommand) = 
    inherit DbCommand() with 
        let beginQuery = new Event<_>()
        let endQuery = new Event<_>()
        let readerCreated = new Event<_>()

        member this.Query f = 
            beginQuery.Trigger(this)
            Timed.action f (fun elapsed -> endQuery.Trigger(this, elapsed))

        override this.CommandText
            with get() = inner.CommandText
            and set(value) = inner.CommandText <- value

        override this.CommandTimeout 
            with get() = inner.CommandTimeout
            and set(value) = inner.CommandTimeout <- value

        override this.CommandType
            with get() = inner.CommandType
            and set(value) = inner.CommandType <- value
            
        override this.DbConnection
            with get() = inner.Connection
            and set(value) = inner.Connection <- value

        override this.DbParameterCollection
            with get() = inner.Parameters

        override this.DbTransaction
            with get() = inner.Transaction
            and set(value) = inner.Transaction <- value

        override this.DesignTimeVisible 
            with get() = inner.DesignTimeVisible
            and set(value) = inner.DesignTimeVisible <- value

        override this.UpdatedRowSource 
            with get() = inner.UpdatedRowSource
            and set(value) = inner.UpdatedRowSource <- value

        override this.Cancel() = inner.Cancel()

        override this.CreateDbParameter() = inner.CreateParameter()

        override this.ExecuteDbDataReader(behavior) =
            let reader = new ProfiledDataReader(this.Query(fun () -> inner.ExecuteReader(behavior)))
            readerCreated.Trigger(reader)
            reader :> DbDataReader

        override this.ExecuteNonQuery() = this.Query inner.ExecuteNonQuery

        override this.ExecuteScalar() = this.Query inner.ExecuteScalar 

        override this.Prepare() = inner.Prepare()

        interface IDisposable with
            member this.Dispose() = 
                inner.Dispose()
                base.Dispose()

        member this.BeginQuery = beginQuery.Publish

        member this.EndQuery = endQuery.Publish

        member this.ReaderCreated = readerCreated.Publish

type ProfiledConnection(inner:DbConnection) =
    inherit DbConnection() with       
        let opening = new Event<_>()
        let closing = new Event<_>()
        let commandCreated = new Event<_>()

        override this.ConnectionString 
            with get() = inner.ConnectionString
            and set(value) = inner.ConnectionString <- value
        
        override this.Database 
            with get() = inner.Database

        override this.DataSource
            with get() = inner.DataSource

        override this.ServerVersion
            with get() = inner.ServerVersion

        override this.State
            with get() = inner.State

        override this.BeginDbTransaction(isolationLevel) = inner.BeginTransaction(isolationLevel)

        override this.Close() = 
            closing.Trigger()
            inner.Close()

        override this.ChangeDatabase(databaseName) = inner.ChangeDatabase(databaseName)
        
        override this.CreateDbCommand() =
            let command = new ProfiledCommand(inner.CreateCommand())
            commandCreated.Trigger(command)
            command :> DbCommand

        override this.Open() = 
            opening.Trigger()
            inner.Open()

        interface IDisposable with
            member this.Dispose() = 
                inner.Dispose()
                base.Dispose()

        member this.Opening = opening.Publish

        member this.Closing = closing.Publish

        member this.CommandCreated = commandCreated.Publish

type IDbProfilingSession =
    abstract BeginQuery : query:ProfiledCommand -> unit
    abstract EndQuery : query:ProfiledCommand * elapsed:TimeSpan -> unit
    abstract BeginRow : reader:ProfiledDataReader -> unit
    abstract EndRow : reader:ProfiledDataReader * elapsed:TimeSpan -> unit

type DbProfilingSession() =
    let mutable queryCount = 0
    let mutable queryTime = TimeSpan.Zero
    let mutable rowCount = 0

    member this.QueryCount = queryCount
    
    member this.QueryTime = queryTime

    member this.RowCount = rowCount

    abstract BeginQuery : query:ProfiledCommand -> unit
    
    default this.BeginQuery query = ()

    abstract EndQuery : query:ProfiledCommand * elapsed:TimeSpan -> unit

    default this.EndQuery(query, elapsed) = ()

    abstract BeginRow : reader:ProfiledDataReader -> unit

    default this.BeginRow(reader) = ()

    abstract EndRow : reader:ProfiledDataReader * elapsed:TimeSpan -> unit

    default this.EndRow(reader, elapsed) = ()

    member this.Reset() =
        queryCount <- 0
        rowCount <- 0
        queryTime <- TimeSpan.Zero

    interface IDbProfilingSession with
        member this.BeginQuery command = 
            queryCount <- queryCount + 1
            this.BeginQuery command

        member this.EndQuery(command, elapsed) = 
            queryTime <- queryTime + elapsed
            this.EndQuery(command, elapsed)

        member this.BeginRow reader = 
            rowCount <- rowCount + 1
            this.BeginRow(reader)

        member this.EndRow(reader, elapsed) =
            this.EndRow(reader, elapsed)

type DbProfiler() =

    member this.Connect(listener:IDbProfilingSession, db) = 
        let db' = new ProfiledConnection(db)
        db'.CommandCreated.Add(fun e -> 
            e.BeginQuery.Add(fun e -> listener.BeginQuery(e))
            e.EndQuery.Add(fun (e,elapsed) -> listener.EndQuery(e, elapsed))
            e.ReaderCreated.Add(fun e -> e.BeginRow.Add(fun e -> listener.BeginRow(e))))
        db'