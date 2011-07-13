namespace Xlnt.Data

open System
open System.Collections.Generic
open System.Text.RegularExpressions

type LinqQueryRewritingSession() =
    static let TablePattern = Regex(@"(\[(?<owner>.+?)\]\.){0,1}\[(?<id>.+?)\] AS \[.+?\]", RegexOptions.Compiled)
    static let OwnerGroup = TablePattern.GroupNumberFromName "owner"
    static let TableIdGroup = TablePattern.GroupNumberFromName "id"
    static let [<Literal>] NoLockPrefix = "#nolock;"

    let nolock = Stack()
    let mutable nolockCache = HashSet()

    let hasNoLockHint (scope:DbProfilingSessionScope) = scope.ScopeName.StartsWith(NoLockPrefix)

    let withHintsFor it x =
        if nolockCache.Contains(it) then
            x + " with(nolock)"
        else x

    let addHints (m:Match) = 
        let table = 
            let id = m.Groups.[TableIdGroup].Value
            if m.Groups.[OwnerGroup].Success then
                m.Groups.[OwnerGroup].Value + "." + id
            else
                id
        m.Value |> withHintsFor table
    
    let unescapeName (s:String) = s.TrimStart([|'['|]).TrimEnd([|']'|])

    let updateCache f = 
        let r = f nolock
        nolockCache <- HashSet(nolock |> (Seq.collect << Seq.map) unescapeName)
        r

    member this.PushNoLockScope ([<ParamArray>] scope) = updateCache (fun x -> x.Push(scope)) 
    member this.PopNoLockScope() = updateCache (fun x -> x.Pop())
    member this.Rewrite query = TablePattern.Replace(query, addHints)

    interface IProfilingSessionQueryListener with
        member this.BeginQuery query =
            query.CommandText <- this.Rewrite query.CommandText
        
        member this.EndQuery(command, elapsed) = ()        
        member this.BeginRow reader = ()        
        member this.EndRow(reader, elapsed) = ()

    interface IProfilingSessionScopeListener with
        member this.EnterScope(oldScope, newScope) =
            if newScope |> hasNoLockHint then   
                this.PushNoLockScope(newScope.ScopeName.Substring(NoLockPrefix.Length).Split([|';'|], StringSplitOptions.RemoveEmptyEntries))

        member this.LeaveScope(oldScope, newScop) = 
            if oldScope |> hasNoLockHint then
                this.PopNoLockScope() |> ignore