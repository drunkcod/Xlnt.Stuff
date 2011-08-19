namespace Xlnt.Data

open System
open System.Text
open System.Data
open System.Data.SqlClient
open System.Resources
open System.IO

[<AutoOpen>]
module Extensions =
    type System.Data.IDbCommand with
        member this.AddParameter(name, value) =
            let parameter = this.CreateParameter()
            parameter.ParameterName <- name
            parameter.Value <- value
            this.Parameters.Add(parameter)

type SqlFileStorage(connectionProvider : unit -> IDbConnection) =
    member this.Initialize() = 
        use db = connectionProvider()
        use cmd = db.CreateCommand()
        use reader = new StreamReader(this.GetType().Assembly.GetManifestResourceStream("SqlFileStorage.Tables.sql"))
        cmd.CommandText <- reader.ReadToEnd()
        db.Open()
        cmd.ExecuteNonQuery() |> ignore

    member this.Insert(path, name) =
        let info = FileInfo(path)
        let bytes = File.ReadAllBytes(info.FullName)
        use db = connectionProvider()
        use cmd = db.CreateCommand()
        use reader = new StreamReader(this.GetType().Assembly.GetManifestResourceStream("SqlFileStorage.InsertFile.sql"))
        cmd.CommandText <- reader.ReadToEnd()
        cmd.AddParameter("@Name", Encoding.UTF8.GetBytes(name:string)) |> ignore
        cmd.AddParameter("@Created", info.CreationTimeUtc) |> ignore
        cmd.AddParameter("@Data", bytes) |> ignore

        db.Open()
        cmd.ExecuteNonQuery() |> ignore

    member this.Compact() =
        use db = connectionProvider()
        use cmd = db.CreateCommand()
        use reader = new StreamReader(this.GetType().Assembly.GetManifestResourceStream("SqlFileStorage.Compact.sql"))
        cmd.CommandText <- reader.ReadToEnd()
        db.Open()
        cmd.ExecuteNonQuery() |> ignore
