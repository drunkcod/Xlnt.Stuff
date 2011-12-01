namespace Xlnt.Data

open System
open System.Collections.Generic

type TableParameters(inner : IProfilingSessionQueryListener) =
    let injectors = Stack()

    new() = TableParameters(NullProfilingSessionListener())

    member this.Add<'a>(xs : 'a seq) = 
        let newParameter = XmlParameter("@" + typeof<'a>.Name)
        newParameter.AddRange(xs)
        injectors.Push(newParameter.Inject)
        {new IDisposable with member x.Dispose() = injectors.Pop() |> ignore }

    interface IProfilingSessionQueryListener with
        member this.BeginBatch query = 
            injectors |> Seq.iter (fun x -> x(query))
            inner.BeginBatch query

        member this.BeginQuery query = inner.BeginQuery query
        member this.BeginRow reader = inner.BeginRow reader
        member this.EndBatch(query, elapsed) = inner.EndBatch(query, elapsed)
        member this.EndQuery(query, elapsed) = inner.EndQuery(query, elapsed)
        member this.EndRow(reader, elapsed) = inner.EndRow(reader, elapsed)
