namespace Xlnt.Data

open System
open System.Collections.Generic
open System.Data
open System.Data.SqlClient
open System.Text
open System.IO
open System.Xml
open System.Linq.Expressions
open System.Reflection

type XmlParameter(parameterName) =
    let [<Literal>] ParameterTagName = "p"
    let ParameterFormat = String.Format("<{0}>{{0}}</{0}>", ParameterTagName)
    let parameters = List()

    let getAttribute name (x : ICustomAttributeProvider) = 
        let attrs = x.GetCustomAttributes(true)
        let rec loop n = 
            if n = attrs.Length then 
                raise(InvalidOperationException())
            else 
                let attr = attrs.[n]
                let attrType = attr.GetType() 
                if attrType.FullName = name then
                    (attr, attrType)
                else loop (n + 1)
        loop 0

    let getName (obj, t:Type) = 
        match t.GetProperty("Name").GetValue(obj, null) with
        | null -> null
        | value -> value.ToString()

    member this.Count = parameters.Count

    member this.Add(value:obj) = parameters.Add(value)

    member this.TempTableDefinition(tableName, columnName, columnType) = 
        let columnType = 
            match columnType with
            | x when x = typeof<String> -> "varchar(max)"
            | x when x = typeof<int> -> "int"
            | x when x = typeof<Guid> -> "uniqueidentifier"
            | _ -> raise(NotSupportedException("Unsupported column type: " + columnType.Name))
        String.Format("select [{0}] = x.value('.', '{4}') into {1} from {2}.nodes('/{3}') _(x)\n", columnName, tableName, parameterName, ParameterTagName, columnType)

    member this.Inject(expr : Expression<Func<'a,'b>>, command) =      
        if expr.Body.NodeType <> ExpressionType.MemberAccess 
        then raise(NotSupportedException("invalid expression type"))
        
        let memberAccess = expr.Body :?> MemberExpression
        this.Inject(this.GetTableName(memberAccess.Member.DeclaringType), this.GetColumnName(memberAccess.Member), expr.Body.Type, command)

    member private this.GetTableName(x : Type) =
        getName <| getAttribute "System.Data.Linq.Mapping.TableAttribute" x
        |> function | null -> x.Name | name -> name

    member private this.GetColumnName(x : MemberInfo) =
        getName <| getAttribute "System.Data.Linq.Mapping.ColumnAttribute" x
        |> function | null -> x.Name | name -> name

    member this.Inject(tableName : String, columnName : String, columnType : Type, command : IDbCommand) =
        command.CommandText <- this.TempTableDefinition(tableName, columnName, columnType) + "\n" + command.CommandText
        command.Parameters.Add(this.ToSqlParameter())

    member private this.GetValue() = 
        let result = StringBuilder()
        for item in parameters do
            result.AppendFormat(ParameterFormat, item) |> ignore
        result.ToString()
    
    member this.ToSqlParameter() = SqlParameter(ParameterName = parameterName, DbType = DbType.Xml, Value = this.GetValue())
