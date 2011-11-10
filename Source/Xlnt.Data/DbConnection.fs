namespace Xlnt.Data

open System
open System.Runtime.CompilerServices
open System.Data

[<Extension>]
module DbConnectionExtensions = 
    [<Extension; CompiledName("ExecuteScalar")>]
    let executeScalar (connection:IDbConnection) query = 
        use command = connection.CreateCommand()
        command.CommandText <- query
        command.ExecuteScalar()