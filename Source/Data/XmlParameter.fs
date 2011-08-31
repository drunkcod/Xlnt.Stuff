namespace Xlnt.Data

open System
open System.Collections.Generic
open System.Data
open System.Data.SqlClient
open System.Text
open System.IO
open System.Xml

type XmlParameter(parameterName) =
    let [<Literal>] ParameterTagName = "p"
    let parameters = List()

    member this.ParameterCount = parameters.Count

    member this.AddParameter(value:obj) = parameters.Add(value)

    member this.TempTableDefinition(tableName, columnName) = 
        String.Format("select [{0}] = x.value('.', 'varchar(max)') into {1} from {2}.nodes('/{3}') _{4}(x)\n", columnName, tableName, parameterName, ParameterTagName, this.GetHashCode())

    member private this.GetValue() = 
        let result = StringBuilder()
        let format = String.Format("<{0}>{{0}}</{0}>", ParameterTagName)
        for item in parameters do
            result.AppendFormat(format, item) |> ignore
        result.ToString()
    
    member this.ToSqlParameter() = SqlParameter(ParameterName = parameterName, DbType = DbType.Xml, Value = this.GetValue())
