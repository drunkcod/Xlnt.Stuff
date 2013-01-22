namespace Xlnt.Data

open System
open System.Collections.Generic
open System.Text
open System.Text.RegularExpressions

type LinqQueryRewritingSession() =
    static let TablePattern = Regex(@"^(?<op>(FROM|INNER JOIN|LEFT OUTER JOIN) )(?<table>(, )?((\[.+?\]\.)?\[.+?\] AS \[.+?\]))+", RegexOptions.Compiled +  RegexOptions.Multiline + RegexOptions.ExplicitCapture)
    static let OpGroup = TablePattern.GroupNumberFromName "op"
    static let TableGroup = TablePattern.GroupNumberFromName "table"
    static let [<Literal>] NoLockPrefix = "#nolock;"

    let nolock = Stack()
    let mutable nolockCache = HashSet().Contains
    let hints = List()
    let mutable queryHints = String.Empty

    let isNoLockHint (scope:DbProfilingSessionScope) = scope.ScopeName.StartsWith(NoLockPrefix)

    let addHints (m:Match) =
        let result = (StringBuilder(m.Groups.[OpGroup].Value))
        m.Groups.[TableGroup].Captures 
        |> Seq.cast
        |> Seq.fold (fun (x:StringBuilder) (table:Capture) -> x.AppendFormat("{0} with(nolock)", table)) result
        |> string
    
    let unescapeName (s:String) = 
        let parts = s.Split([|'.'|])
        for i = 0 to parts.Length - 1 do
            parts.[i] <- parts.[i].TrimStart([|'['|]).TrimEnd([|']'|])
        String.Join(".", parts)

    let updateCache f = 
        let r = f nolock
        let tables = nolock |> (Seq.collect << Seq.map) unescapeName
        nolockCache <- HashSet(tables).Contains
        r

    let updateQueryHints f =
        f hints
        queryHints <- "\noption(" + String.Join(",", hints) + ")"

    member this.AddHint hint  = 
        updateQueryHints (fun x -> x.Add(hint))

    member this.PushNoLockAll() = 
        nolock.Push([||])
        nolockCache <- fun x -> true

    member this.PushNoLockScope ([<ParamArray>] scope) = updateCache (fun x -> x.Push(scope)) 
    member this.PopNoLockScope() = updateCache (fun x -> x.Pop())
    member this.Rewrite query =
        let query = TablePattern.Replace(query, addHints)
        if hints.Count = 0 then
            query
        else query + queryHints

    interface IProfilingSessionQueryListener with
        member this.BeginQuery query =
            query.CommandText <- this.Rewrite query.CommandText
        
        member this.EndQuery(command, elapsed) = ()        
        member this.BeginRow reader = ()        
        member this.EndRow(reader, elapsed) = ()

    interface IProfilingSessionScopeListener with
        member this.EnterScope(oldScope, newScope) =
            if newScope |> isNoLockHint then   
                this.PushNoLockScope(newScope.ScopeName.Substring(NoLockPrefix.Length).Split([|';'|], StringSplitOptions.RemoveEmptyEntries))

        member this.LeaveScope(oldScope, newScop) = 
            if oldScope |> isNoLockHint then
                this.PopNoLockScope() |> ignore

module QueryHint =
    [<Literal>] let Recompile = "recompile"
