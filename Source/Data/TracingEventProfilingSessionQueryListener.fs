﻿namespace Xlnt.Data

open System
open System.Data

type QueryEventArgs(command:IDbCommand, elapsed) =
    inherit EventArgs()    

    member this.CommandText = command.CommandText
    member this.Elapsed = elapsed;

    override this.ToString() = this.CommandText

type CLIEvent<'T> when 'T :> EventArgs = Event<EventHandler<'T>,'T>

type TracingEventProfilingSessionQueryListener(inner:IProfilingSessionQueryListener) =
    let beginQuery = CLIEvent()
    let endQuery = CLIEvent()

    [<CLIEvent>] member this.BeginQuery = beginQuery.Publish
    [<CLIEvent>] member this.EndQuery = endQuery.Publish

    interface IProfilingSessionQueryListener with
        member this.BeginQuery query =
            beginQuery.Trigger(this, QueryEventArgs(query, TimeSpan.Zero))
            inner.BeginQuery query

        member this.EndQuery(query, elapsed) = 
            inner.EndQuery(query, elapsed)
            endQuery.Trigger(this, QueryEventArgs(query, elapsed))
    
        member this.BeginRow reader = inner.BeginRow reader
        member this.EndRow(reader, elapsed) = inner.EndRow(reader, elapsed)