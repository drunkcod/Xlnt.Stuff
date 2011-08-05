// Learn more about F# at http://fsharp.net
namespace SqlCmdSharp.Console

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
        | None | Some(Trusted) -> Some(UserPass(user, String.Empty))
        | Some(UserPass(_, password)) -> Some(UserPass(user, password))

    let withPassword password = function
        | None | Some(Trusted) -> Some(UserPass(String.Empty, password))
        | Some(UserPass(user, _)) -> Some(UserPass(user, password))

    type InputSource =
        | QueryAndExit of string
        | InputFile of string

    type ProgramOptions = {
        Server : string option
        Database : string option 
        Connection : ConnectionType option 
        Input : InputSource option }
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

    let parseOptions args =
        let rec loop opt = function
            | [] -> opt
            | "-S"::server::xs -> loop {opt with Server = Some(server) } xs
            | "-d"::database::xs -> loop {opt with Database = Some(database) } xs
            | "-E"::xs -> loop { opt with Connection = Some(Trusted) } xs
            | "-U"::username::xs -> loop { opt with Connection = opt.Connection |> withUser username } xs
            | "-P"::password::xs -> loop { opt with Connection = opt.Connection |> withPassword password } xs
            | "-Q"::query::xs -> loop { opt with Input = Some(QueryAndExit(query)) } xs
            | "-i"::inputfile::xs -> loop { opt with Input = Some(InputFile(inputfile)) } xs
            | _ -> raise(NotSupportedException())
        loop { Server = None; Database = None; Connection = None; Input = None } args

    [<EntryPoint>]
    let main(args) =
        let options = parseOptions (args |> Array.toList)
        use db = new SqlConnection(options.ConnectionString)
        db.Open()
        options.Input |> Option.iter (function
        | QueryAndExit(command) -> 
            use cmd = db.CreateCommand()
            cmd.CommandText <- command
            cmd.ExecuteNonQuery() |> ignore
        | InputFile(path) ->
            use reader = new StreamReader(path, Encoding.Default)
            SqlCmdSharp.readQueryBatches reader (fun batch -> 
                use cmd = db.CreateCommand()
                cmd.CommandText <- batch
                cmd.ExecuteNonQuery() |> ignore)
        | _ -> ())

        0