namespace Xlnt.Data

open System
open System.Collections.Generic

type Injector<'a> =
    | Empty
    | Injector of ('a -> unit) * Injector<'a>
with 
    member this.Combine x = Injector(x, this)
    
    member this.Pop() = 
        match this with
        | Empty -> raise(InvalidOperationException())
        | Injector(_, inner) -> inner

    member this.Invoke x =
        let rec loop = function 
            | Empty -> () 
            | Injector(f, next) ->
                f(x)
                loop next
        loop this

type TableParameters(inner : IProfilingSessionBatchListener) =
    let mutable injector = Empty

    new() = TableParameters(NullProfilingSessionListener())

    member this.Add<'a>(xs : 'a seq) = 
        let newParameter = XmlParameter("@" + typeof<'a>.Name)
        newParameter.AddRange(xs)
        injector <- injector.Combine(newParameter.Inject)
        {new IDisposable with member x.Dispose() = injector <- injector.Pop() }

    interface IProfilingSessionBatchListener with
        member this.BeginBatch query = 
            injector.Invoke(query)
            inner.BeginBatch query

        member this.EndBatch(query, elapsed) = inner.EndBatch(query, elapsed)
