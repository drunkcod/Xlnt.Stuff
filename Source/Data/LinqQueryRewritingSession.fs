namespace Xlnt.Data

open System
open System.Collections.Generic
open System.Text.RegularExpressions

type LinqQueryRewritingSession() =
    static let TablePattern = Regex(@"(\[(?<owner>.+?)\]\.)?\[(?<id>.+?)\] AS \[.+?\]", RegexOptions.Compiled)
    static let OwnerGroup = TablePattern.GroupNumberFromName "owner"
    static let TableIdGroup = TablePattern.GroupNumberFromName "id"
    static let [<Literal>] NoLockPrefix = "#nolock;"

    let nolock = Stack()
    let mutable nolockCache = HashSet()
    let mutable nolockAll = false

    let isNoLockHint (scope:DbProfilingSessionScope) = scope.ScopeName.StartsWith(NoLockPrefix)

    let withHintsFor it x =
        if nolockCache.Contains(it) then
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
        nolockCache <- HashSet(tables)
        r

    member this.PushNoLockScope ([<ParamArray>] scope) = updateCache (fun x -> x.Push(scope)) 
    member this.PopNoLockScope() = updateCache (fun x -> x.Pop())
    member this.Rewrite query = TablePattern.Replace(query, addHints)

    interface IProfilingSessionQueryListener with
        member this.BeginBatch query = ()
        member this.EndBatch(query, elapsed) = ()

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