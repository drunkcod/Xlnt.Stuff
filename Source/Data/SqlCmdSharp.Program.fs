﻿namespace SqlCmdSharp.Console

open System
open System.Text;
open System.IO
open System.Data.SqlClient
open Xlnt.Data

module Program =
    type ConnectionType =
        | Trusted
        | UserPass of string * string

    let withUser user = function
        | Some(UserPass(_, password)) -> UserPass(user, password)
        | _ -> UserPass(user, String.Empty)

    let withPassword password = function
        | Some(UserPass(user, _)) -> UserPass(user, password)
        | _ -> UserPass(String.Empty, password)

    type Action =
        | QueryAndExit of string
        | InputFile of string

    type ProgramOptions = {
        Server : string option
        Database : string option 
        Connection : ConnectionType option 
        Action : Action option }
    with 
        member this.ConnectionString =
            let builder = SqlConnectionStringBuilder()
            builder.ApplicationName <- "SqlCmd#"
            builder.DataSource <- Option.get this.Server
            this.Database |> Option.iter (fun x -> builder.InitialCatalog <- x)
            this.Connection |> Option.iter (function
            | Trusted -> builder.IntegratedSecurity <- true
            | UserPass(user, password) -> 
                builder.UserID <- user
                builder.Password <- password)
            builder.ConnectionString
        static member Parse args =
            let parseArg(opt, xs)= 
                match xs with 
                | "-S"::server::xs -> ({opt with Server = Some(server) }, xs)
                | "-d"::database::xs -> ({opt with Database = Some(database) }, xs)
                | "-E"::xs -> ({ opt with Connection = Some(Trusted) }, xs)
                | "-U"::username::xs -> ({ opt with Connection = Some(opt.Connection |> withUser username) }, xs)
                | "-P"::password::xs -> ({ opt with Connection = Some(opt.Connection |> withPassword password) }, xs)
                | "-Q"::query::xs -> ({ opt with Action = Some(QueryAndExit(query)) }, xs)
                | "-i"::inputfile::xs -> ({ opt with Action = Some(InputFile(inputfile)) }, xs)
                | _ -> raise(NotSupportedException())
            let rec loop = function
                | (x, []) -> x
                | x -> parseArg x |> loop 
            loop({ Server = None; Database = None; Connection = None; Action = None }, args)

    [<EntryPoint>]
    let main(args) =
        let options = ProgramOptions.Parse (args |> Array.toList)
        use db = new SqlConnection(options.ConnectionString)
        let executeNonQuery query =
            use cmd = db.CreateCommand()
            cmd.CommandText <- query
            cmd.ExecuteNonQuery() |> ignore
        try 
            db.Open()
            options.Action |> Option.iter (function
            | QueryAndExit(command) -> executeNonQuery command
            | InputFile(path) ->
                use reader = new StreamReader(path, Encoding.Default)
                SqlCmdSharp.readQueryBatches reader executeNonQuery)
            0
        with e ->
            -1
        