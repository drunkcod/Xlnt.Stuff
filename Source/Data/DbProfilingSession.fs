﻿namespace Xlnt.Data

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

type IProfilingSessionQueryListener =   
    abstract BeginQuery : query:ProfiledCommand -> unit
    abstract EndQuery : query:ProfiledCommand * elapsed:TimeSpan -> unit
    abstract BeginRow : reader:ProfiledDataReader -> unit
    abstract EndRow : reader:ProfiledDataReader * elapsed:TimeSpan -> unit

type DbProfilingSession(queryListener:IProfilingSessionQueryListener, scopeListener:IProfilingSessionScopeListener) as this =
    let globalScope = new DbProfilingSessionScope("<global>", this, None)
    let mutable currentScope = globalScope

    member this.QueryCount = globalScope.QueryCount
    member this.RowCount = globalScope.RowCount
    member this.QueryTime = globalScope.QueryTime

    member this.BeginQuery query = 
        currentScope.Query()
        queryListener.BeginQuery query
    
    member this.EndQuery(query, elapsed) = 
        currentScope.QueryElapsed elapsed
        queryListener.EndQuery(query, elapsed)

    member this.BeginRow reader = queryListener.BeginRow reader 

    member this.EndRow(reader, elapsed) = 
        currentScope.Row()
        queryListener.EndRow(reader, elapsed)

    member this.Scoped name action = currentScope.Scoped name action

    new() = DbProfilingSession({ new IProfilingSessionQueryListener with
            member this.BeginQuery command = ()
            member this.EndQuery(command, elapsed) = ()
            member this.BeginRow reader = ()
            member this.EndRow(reader, elapsed) = ()
        }, { new IProfilingSessionScopeListener with
            member this.EnterScope(oldScope, newScope) = ()
            member this.LeaveScope(oldScope, newScope) = () 
        })

    interface IProfilingSessionScopeListener with 
        member this.EnterScope(oldScope, newScope) = 
            currentScope <- newScope
            scopeListener.EnterScope(oldScope, newScope)

        member this.LeaveScope(oldScope, newScope) =
            scopeListener.LeaveScope(oldScope, newScope)
            currentScope <- newScope