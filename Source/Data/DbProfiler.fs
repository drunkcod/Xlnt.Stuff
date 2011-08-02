namespace Xlnt.Data

open System
open System.Data
open System.Data.Common
open System.Diagnostics
open System.Threading
open Xlnt

module DbProfiler =
    [<CompiledName("Connect")>]
    let connect (listener:DbProfilingSession) db = 
        let db' = new ProfiledConnection(db)
        db'.CommandCreated.Add(fun command ->
            command.BeginQuery.Add(fun e -> 
                listener.BeginBatch(e)
                listener.BeginQuery(e))
            command.EndQuery.Add(fun (e, elapsed) -> listener.EndQuery(e, elapsed))
            command.ReaderCreated.Add(fun reader -> 
                let batchTimer = Stopwatch.StartNew()
                reader.BeginRow.Add(listener.BeginRow)
                reader.EndRow.Add(listener.EndRow)
                reader.Closed.Add(fun _ -> 
                    batchTimer.Stop()
                    listener.EndBatch(command, batchTimer.Elapsed))))
        db'