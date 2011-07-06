namespace Xlnt.Data

open System
open System.Collections.Generic
open System.Text.RegularExpressions

type LinqQueryRewritingSession() =
    static let TablePattern = Regex(@"\[(?<id>.+?)\] AS \[.+?\]", RegexOptions.Compiled)
    static let TableIdGroup = TablePattern.GroupNumberFromName "id"
    static let [<Literal>] NoLockPrefix = "#nolock;"

    let nolock = Stack()
    let mutable nolockCache = HashSet()

    let isNoLockHint (scope:DbProfilingSessionScope) = scope.ScopeName.StartsWith(NoLockPrefix)

    let ifNoLockScope f scope = 
        if isNoLockHint scope then
            f scope
            nolockCache <- HashSet(nolock |> Seq.collect id)

    let withHintsFor it x =
        if nolockCache.Contains(it) then
            x + " with(nolock)"
        else x

    let addHints (m:Match) = m.Value |> withHintsFor m.Groups.[TableIdGroup ].Value
 
    member this.PushNoLockScope scope = nolock.Push(scope)

    member this.PopNoLockScope() = nolock.Pop()

    member this.Rewrite query = TablePattern.Replace(query, addHints)

    interface IProfilingSessionQueryListener with
        member this.BeginQuery query =
            query.CommandText <- this.Rewrite query.CommandText
        
        member this.EndQuery(command, elapsed) = ()
        
        member this.BeginRow reader = ()
        
        member this.EndRow(reader, elapsed) = ()

    interface IProfilingSessionScopeListener with
        member this.EnterScope(oldScope, newScope) =
            newScope |> ifNoLockScope (fun scope -> this.PushNoLockScope(scope.ScopeName.Substring(NoLockPrefix.Length).Split([|';'|], StringSplitOptions.RemoveEmptyEntries)))

        member this.LeaveScope(oldScope, newScop) = oldScope |> ifNoLockScope (fun _ -> this.PopNoLockScope() |> ignore)