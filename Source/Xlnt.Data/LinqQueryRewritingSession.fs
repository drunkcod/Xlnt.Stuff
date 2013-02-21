namespace Xlnt.Data

open System
open System.Data
open System.Collections.Generic
open System.Text
open System.Text.RegularExpressions

type QueryHint =
    Recompile

type SqlFormatting = {
    NolockFormat : string
    OptionFormat : string
    FormatQueryHint : QueryHint -> String
}

type LinqQueryRewritingSession() =

    static let TablePattern = Regex(@"^(?<op>\s*(FROM|INNER JOIN|LEFT OUTER JOIN) )(?<table>(, )?((\[.+?\]\.)?\[.+?\] AS \[.+?\]))+", RegexOptions.Compiled +  RegexOptions.Multiline + RegexOptions.ExplicitCapture)
    static let OpGroup = TablePattern.GroupNumberFromName "op"
    static let TableGroup = TablePattern.GroupNumberFromName "table"
    static let Nop = id<string>

    static let defaultFormatting = {
        NolockFormat = "{0} with(nolock)"
        OptionFormat = "\r\noption({0})"
        FormatQueryHint = function
            | Recompile -> "recompile"
    }

    let hints = List()
    let mutable nolock = Nop
    let mutable queryHints = String.Empty
    let mutable prologue = String.Empty
    let mutable format = defaultFormatting

    let addHints (m:Match) =
        let result = (StringBuilder(m.Groups.[OpGroup].Value))
        m.Groups.[TableGroup].Captures 
        |> Seq.cast
        |> Seq.fold (fun (x:StringBuilder) (table:Capture) -> x.AppendFormat(format.NolockFormat, table)) result
        |> (fun x -> x.ToString())

    let updateQueryHints f =
        f hints
        queryHints <- String.Format(format.OptionFormat, String.Join(",", hints |> Seq.map format.FormatQueryHint))

    member this.Prologue 
        with get() = prologue 
        and set value = prologue <- value

    member this.Nolock
        with get() = not(Object.ReferenceEquals(nolock, Nop))
        and set value =
            match value with
            | true -> nolock <- fun query -> TablePattern.Replace(query, addHints)
            | false -> nolock <- id

    member this.UseTraditionalFormatting
        with get() = not(Object.ReferenceEquals(format, defaultFormatting))
        and set value = 
            match value with
            | true -> format <- {
                NolockFormat = "{0} WITH(NOLOCK)"
                OptionFormat = "\r\nOPTION({0})" 
                FormatQueryHint = function
                    | Recompile -> "RECOMPILE"
              }
            | false -> format <- defaultFormatting
            

    member this.AddHint hint  = 
        updateQueryHints (fun x -> x.Add(hint))

    member this.RemoveHint hint = 
        updateQueryHints (fun x -> x.Remove(hint) |> ignore)

    member this.Rewrite query =
        let q = 
            let query = nolock(query)
            if hints.Count = 0 then
                query
            else query + queryHints
        if prologue |> String.IsNullOrEmpty then
            q
        else prologue + "\n" + q

    interface IProfilingSessionQueryListener with
        member this.BeginQuery query =
            if query.CommandType = CommandType.Text then
                query.CommandText <- this.Rewrite query.CommandText
        
        member this.EndQuery(command, elapsed) = ()        
        member this.BeginRow reader = ()        
        member this.EndRow(reader, elapsed) = ()