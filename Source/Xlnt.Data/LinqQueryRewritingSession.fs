namespace Xlnt.Data

open System
open System.Collections.Generic
open System.Text.RegularExpressions

type LinqQueryRewritingSession() =
    static let TablePattern = Regex(@"(FROM|JOIN) ((\[(?<owner>.+?)\]\.)?\[(?<id>.+?)\] AS \[.+?\])|, ((\[(?<owner>.+?)\]\.)?\[(?<id>.+?)\] AS \[.+?\])", RegexOptions.Compiled)
    static let OwnerGroup = TablePattern.GroupNumberFromName "owner"
    static let TableIdGroup = TablePattern.GroupNumberFromName "id"
    static let [<Literal>] NoLockPrefix = "#nolock;"

    let nolock = Stack()
    let mutable nolockCache = HashSet().Contains
    let hints = List()
    let mutable queryHints = String.Empty

    let isNoLockHint (scope:DbProfilingSessionScope) = scope.ScopeName.StartsWith(NoLockPrefix)

    let withHintsFor it x =
        if nolockCache(it) then
            x + " with(nolock)"
        else x

    let addHints (m:Match) = 
        let table = 
            let id = m.Groups.[TableIdGroup].Value
            let owner = m.Groups.[OwnerGroup]
            if owner.Success then
                owner.Value + "." + id
            else id
        m.Value |> withHintsFor table
    
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
