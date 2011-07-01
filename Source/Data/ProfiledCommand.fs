namespace Xlnt.Data

open System
open System.Data.Common
open Xlnt

type ProfiledCommand(inner:DbCommand) = 
    inherit DbCommand() with 
        let beginQuery = new Event<_>()
        let endQuery = new Event<_>()
        let readerCreated = new Event<_>()

        member this.Query f = 
            beginQuery.Trigger(this)
            Timed.action f (fun (_, elapsed) -> endQuery.Trigger(this, elapsed))

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

