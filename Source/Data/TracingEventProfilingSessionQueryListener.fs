namespace Xlnt.Data

open System
open System.Collections.Generic
open System.Data
open System.Diagnostics

type QueryParameters = { ParameterName : String; Value : obj }

type QueryEventArgs(command:IDbCommand, elapsed) =
    inherit EventArgs()    

    member this.CommandText = command.CommandText
    member this.CommandTimeout 
        with get() = TimeSpan.FromSeconds((float)command.CommandTimeout)
        and set(value:TimeSpan) = command.CommandTimeout <- (int)value.TotalSeconds
    member this.Elapsed = elapsed;
    member this.Parameters = command.Parameters |> Seq.cast<IDataParameter> |> Seq.map (fun x -> { ParameterName = x.ParameterName; Value = x.Value })

    override this.ToString() = this.CommandText

type RowEventArgs(reader:IDataReader, elapsed) =
    inherit EventArgs()

    member this.Elapsed = elapsed      

type CLIEvent<'T> when 'T :> EventArgs = Event<EventHandler<'T>,'T>

type TracingEventProfilingSessionQueryListener(inner:IProfilingSessionQueryListener) =
    let beginBatch = CLIEvent()
    let endBatch = CLIEvent()
    let beginQuery = CLIEvent()
    let endQuery = CLIEvent()
    let beginRow = CLIEvent()
    let endRow = CLIEvent()

    new() = TracingEventProfilingSessionQueryListener(NullProfilingSessionListener())

    [<CLIEvent>] member this.BeginBatch = beginBatch.Publish
    [<CLIEvent>] member this.EndBatch = endBatch.Publish
    [<CLIEvent>] member this.BeginQuery = beginQuery.Publish
    [<CLIEvent>] member this.EndQuery = endQuery.Publish
    [<CLIEvent>] member this.BeginRow = beginRow.Publish
    [<CLIEvent>] member this.EndRow = endRow.Publish

    interface IProfilingSessionQueryListener with
        member this.BeginBatch query =
            beginBatch.Trigger(this, QueryEventArgs(query, TimeSpan.Zero))
            inner.BeginBatch query

        member this.EndBatch(query, elapsed)=
            inner.EndBatch(query, elapsed)
            endBatch.Trigger(this, QueryEventArgs(query, elapsed))

        member this.BeginQuery query =
            beginQuery.Trigger(this, QueryEventArgs(query, TimeSpan.Zero))
            inner.BeginQuery query

        member this.EndQuery(query, elapsed) = 
            inner.EndQuery(query, elapsed)
            endQuery.Trigger(this, QueryEventArgs(query, elapsed))
    
        member this.BeginRow reader = 
            beginRow.Trigger(this, RowEventArgs(reader, TimeSpan.Zero))
            inner.BeginRow reader
        
        member this.EndRow(reader, elapsed) = 
            inner.EndRow(reader, elapsed)
            endRow.Trigger(this, RowEventArgs(reader, elapsed))