namespace Xlnt.Data

open System
open System.Collections.Generic

type IProfilingSessionScopeListener = 
    abstract EnterScope : scope:DbProfilingSessionScope -> unit
    abstract ExitScope : scope:DbProfilingSessionScope -> unit

and DbProfilingSessionScope(name, listener:IProfilingSessionScopeListener) =
    let mutable queryCount = 0
    let mutable queryTime = TimeSpan.Zero
    let mutable rowCount = 0
    let children = List()
    
    member this.ScopeName = name

    member this.QueryCount = queryCount
    
    member this.QueryTime = queryTime

    member this.RowCount = rowCount

    member this.Reset() =
        queryCount <- 0
        rowCount <- 0
        queryTime <- TimeSpan.Zero

    member internal this.Detach child = 
        children.Remove child |> ignore
        listener.ExitScope child

    member this.Enter name = 
        let child = new DbProfilingSessionScope(name, listener)
        children.Add(child)
        listener.EnterScope child
        child

    member internal this.Query() = 
        queryCount <- queryCount + 1
        children.ForEach(fun x -> x.Query())

    member internal this.QueryElapsed elapsed = 
        queryTime <- queryTime + elapsed
        children.ForEach(fun x -> x.QueryElapsed elapsed)

    member internal this.Row() = 
        rowCount <- rowCount + 1
        children.ForEach(fun x -> x.Row())

    interface IDisposable with
        member this.Dispose() = this.Detach this     

type IDbProfilingSession =   
    abstract BeginQuery : query:ProfiledCommand -> unit
    abstract EndQuery : query:ProfiledCommand * elapsed:TimeSpan -> unit
    abstract BeginRow : reader:ProfiledDataReader -> unit
    abstract EndRow : reader:ProfiledDataReader * elapsed:TimeSpan -> unit

type DbProfilingSession() as this =
    let scope = new DbProfilingSessionScope("<global>", this)

    member this.QueryCount = scope.QueryCount

    member this.RowCount = scope.RowCount

    member this.QueryTime = scope.QueryTime

    abstract BeginQuery : query:ProfiledCommand -> unit
    
    default this.BeginQuery query = ()

    abstract EndQuery : query:ProfiledCommand * elapsed:TimeSpan -> unit

    default this.EndQuery(query, elapsed) = ()

    abstract BeginRow : reader:ProfiledDataReader -> unit

    default this.BeginRow(reader) = ()

    abstract EndRow : reader:ProfiledDataReader * elapsed:TimeSpan -> unit

    default this.EndRow(reader, elapsed) = ()

    member this.EnterScope name = scope.Enter(name)

    abstract EnterScope : scope:DbProfilingSessionScope -> unit

    default this.EnterScope scope = ()

    abstract ExitScope : scope:DbProfilingSessionScope -> unit

    default this.ExitScope scope = ()

    interface IDbProfilingSession with
        member this.BeginQuery command = 
            scope.Query()
            this.BeginQuery command

        member this.EndQuery(command, elapsed) = 
            scope.QueryElapsed elapsed
            this.EndQuery(command, elapsed)

        member this.BeginRow reader = 
            this.BeginRow(reader)

        member this.EndRow(reader, elapsed) =
            scope.Row()
            this.EndRow(reader, elapsed)

    interface IProfilingSessionScopeListener with 
        member this.EnterScope scope = this.EnterScope scope

        member this.ExitScope scope = this.ExitScope scope