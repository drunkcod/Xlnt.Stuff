namespace Xlnt.Data

open System
open System.Data
open System.Data.Common
open System.Threading
open Xlnt

module DbProfiler =
    [<CompiledName("Connect")>]
    let connect (listener:DbProfilingSession) db = 
        let db' = new ProfiledConnection(db)
        db'.CommandCreated.Add(fun e ->
            e.BeginQuery.Add(fun e -> listener.BeginQuery(e))
            e.EndQuery.Add(fun (e, elapsed) -> listener.EndQuery(e, elapsed))
            e.ReaderCreated.Add(fun e -> 
                e.BeginRow.Add(listener.BeginRow)
                e.EndRow.Add(listener.EndRow)))
        db'