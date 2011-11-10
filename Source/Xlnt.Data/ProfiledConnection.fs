namespace Xlnt.Data

open System
open System.Data.Common

type ProfiledConnection(inner:DbConnection) =
    inherit DbConnection() with       
        let opening = new Event<_>()
        let closing = new Event<_>()
        let commandCreated = new Event<_>()

        override this.ConnectionString 
            with get() = inner.ConnectionString
            and set(value) = inner.ConnectionString <- value
        
        override this.Database = inner.Database
        override this.DataSource = inner.DataSource
        override this.ServerVersion = inner.ServerVersion
        override this.State = inner.State

        override this.BeginDbTransaction(isolationLevel) = new ProfiledTransaction(this, inner.BeginTransaction(isolationLevel)) :> DbTransaction

        override this.Close() = 
            closing.Trigger()
            inner.Close()

        override this.ChangeDatabase(databaseName) = inner.ChangeDatabase(databaseName)
        
        override this.CreateDbCommand() =
            let command = new ProfiledCommand(inner.CreateCommand())
            commandCreated.Trigger(command)
            command :> DbCommand

        override this.EnlistTransaction transaction = inner.EnlistTransaction transaction

        override this.Open() = 
            opening.Trigger()
            inner.Open()

        override this.Dispose disposing =
            if disposing then
                inner.Dispose()
            base.Dispose disposing

        member this.Opening = opening.Publish
        member this.Closing = closing.Publish
        member this.CommandCreated = commandCreated.Publish
