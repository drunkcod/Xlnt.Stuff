namespace Xlnt.Data

open System
open System.Collections.Generic

type IProfilingSessionScopeListener = 
    abstract EnterScope : oldScope:DbProfilingSessionScope * newScope:DbProfilingSessionScope -> unit
    abstract LeaveScope : oldScope:DbProfilingSessionScope * newScope:DbProfilingSessionScope -> unit

and DbProfilingSessionScope(name, listener:IProfilingSessionScopeListener, outerScope) =
    let mutable queryCount = 0
    let mutable queryTime = TimeSpan.Zero
    let mutable rowCount = 0

    member this.ScopeName = name

    member this.QueryCount = queryCount
    
    member this.QueryTime = queryTime

    member this.RowCount = rowCount

    member this.Scoped name (action:Action<_>) =
        use child = this.Enter name
        action.Invoke(child)
        child

    member this.Enter name = 
        let child = new DbProfilingSessionScope(name, listener, Some(this))
        listener.EnterScope(this, child)
        child

    member this.Leave() = 
        listener.LeaveScope(this, Option.get outerScope)

    member internal this.Query() =        
        queryCount <- queryCount + 1
        outerScope |> Option.iter (fun x -> x.Query())

    member internal this.QueryElapsed elapsed = 
        queryTime <- queryTime + elapsed
        outerScope |> Option.iter (fun x -> x.QueryElapsed elapsed)

    member internal this.Row() = 
        rowCount <- rowCount + 1
        outerScope |> Option.iter (fun x -> x.Row())

    interface IDisposable with
        member this.Dispose() = this.Leave()

type IDbProfilingSession =   
    abstract BeginQuery : query:ProfiledCommand -> unit
    abstract EndQuery : query:ProfiledCommand * elapsed:TimeSpan -> unit
    abstract BeginRow : reader:ProfiledDataReader -> unit
    abstract EndRow : reader:ProfiledDataReader * elapsed:TimeSpan -> unit

type DbProfilingSession() as this =
    let globalScope = new DbProfilingSessionScope("<global>", this, None)
    let mutable currentScope = globalScope

    member this.QueryCount = globalScope.QueryCount

    member this.RowCount = globalScope.RowCount

    member this.QueryTime = globalScope.QueryTime

    abstract BeginQuery : query:ProfiledCommand -> unit
    
    default this.BeginQuery query = ()

    abstract EndQuery : query:ProfiledCommand * elapsed:TimeSpan -> unit

    default this.EndQuery(query, elapsed) = ()

    abstract BeginRow : reader:ProfiledDataReader -> unit

    default this.BeginRow(reader) = ()

    abstract EndRow : reader:ProfiledDataReader * elapsed:TimeSpan -> unit

    default this.EndRow(reader, elapsed) = ()

    member this.Scoped name action = currentScope.Scoped name action

    abstract EnterScope : scope:DbProfilingSessionScope -> unit

    default this.EnterScope scope = ()

    abstract LeaveScope : scope:DbProfilingSessionScope -> unit

    default this.LeaveScope scope = ()

    interface IDbProfilingSession with
        member this.BeginQuery command = 
            currentScope.Query()
            this.BeginQuery command

        member this.EndQuery(command, elapsed) = 
            currentScope.QueryElapsed elapsed
            this.EndQuery(command, elapsed)

        member this.BeginRow reader = 
            this.BeginRow(reader)

        member this.EndRow(reader, elapsed) =
            currentScope.Row()
            this.EndRow(reader, elapsed)

    interface IProfilingSessionScopeListener with 
        member this.EnterScope(oldScope, newScope) = 
            currentScope <- newScope
            this.EnterScope currentScope

        member this.LeaveScope(oldScope, newScope) = 
            this.LeaveScope oldScope
            currentScope <- newScope