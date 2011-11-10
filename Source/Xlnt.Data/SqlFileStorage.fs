namespace Xlnt.Data

open System
open System.Text
open System.Data
open System.Data.SqlClient
open System.Resources
open System.IO

type SqlFileStorage(connectionProvider : unit -> IDbConnection) =
    member this.ResourceQuery name f =
        use db = connectionProvider()
        use cmd = db.CreateCommand()
        use reader = new StreamReader(this.GetType().Assembly.GetManifestResourceStream(name))
        cmd.CommandText <- reader.ReadToEnd()      
        db.Open()
        f(cmd)

    member this.Initialize() = 
        this.ResourceQuery "SqlFileStorage.Tables.sql" (fun cmd -> cmd.ExecuteNonQuery() |> ignore)

    member this.Insert(path, name) =
        let info = FileInfo(path)
        let bytes = File.ReadAllBytes(info.FullName)
        this.ResourceQuery "SqlFileStorage.InsertFile.sql" (fun cmd -> 
            cmd.AddParameter("@Name", Encoding.UTF8.GetBytes(name:string)) |> ignore
            cmd.AddParameter("@Created", info.CreationTimeUtc) |> ignore
            cmd.AddParameter("@Data", bytes) |> ignore
            cmd.ExecuteNonQuery() |> ignore)

    member this.Compact() =
        this.ResourceQuery "SqlFileStorage.Compact.sql" (fun cmd -> cmd.ExecuteNonQuery() |> ignore)
