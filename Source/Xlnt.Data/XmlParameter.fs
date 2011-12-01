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

[<AutoOpen>]
module private Expressions =
    let (|MemberAccess|_|) (x : Expression) = 
        match x.NodeType with
        | ExpressionType.MemberAccess -> Some(MemberAccess(x :?> MemberExpression))
        | _ -> None 

type private ParameterHolder<'a> =
    | Empty
    | ItemList of 'a List
    | Sequence of 'a seq

type ParameterCollection<'a>() =
    let mutable holder = Empty
    let toList xs = 
        let x = List()
        x.AddRange(xs)
        holder <- ItemList(x)
        x

    member this.GetEnumerator() =
        match holder with
        | Empty -> Seq.empty.GetEnumerator()
        | ItemList(xs) -> xs.GetEnumerator() :> IEnumerator<'a>
        | Sequence(xs) -> xs.GetEnumerator()

    member this.AddRange items =
        match holder with
        | Empty -> holder <- Sequence(items)        
        | ItemList(xs) -> xs.AddRange(items)
        | Sequence(xs) -> toList(xs).AddRange items

    member this.Add item =
        match holder with
        | Empty -> holder <- Sequence([|item|])
        | ItemList(xs) -> xs.Add(item)
        | Sequence(xs) -> toList(xs).Add item

type XmlParameterFormatter = { ColumnName : string; Append : Action<StringBuilder, obj>; Type : Type }

module XmlParameter =
    let [<Literal>] ParameterTagName = "p"
    let ParameterFormat = String.Format("<{0}>{{0}}</{0}>", ParameterTagName)
    
    let internal getAttribute name (x: ICustomAttributeProvider) = 
        let attrs = x.GetCustomAttributes(true)
        let rec loop n = 
            if n = attrs.Length then 
                raise(InvalidOperationException())
            else 
                let attr = attrs.[n]
                let attrType = attr.GetType() 
                if attrType.Name = name then
                    (attr, attrType)
                else loop (n + 1)
        loop 0

    let internal getName(obj, t : Type) = 
        match t.GetProperty("Name").GetValue(obj, null) with
        | null -> null
        | value -> value.ToString()

    let inline internal defaultToName (x :^a) = function null -> (^a : (member Name : string)(x)) | _ as x -> x
    let inline internal getNameAttribute attr x = getAttribute attr x |> getName |> defaultToName x 

    let internal getTableName(x : Type) = getNameAttribute "TableAttribute" x
    let internal getColumnName(x : MemberInfo) = getNameAttribute "ColumnAttribute" x
    
    let private singleColumnFormatter(t : Type, column : MemberInfo, columnType) =
        let target = Expression.Parameter(typeof<StringBuilder>, "target")
        let value = Expression.Parameter(typeof<obj>, "value")
        let getColumn = Expression.Convert(Expression.MakeMemberAccess(Expression.Convert(value, t), column), typeof<obj>)
        let body =
            Expression.Call(target, typeof<StringBuilder>.GetMethod("AppendFormat", [|typeof<string>; typeof<obj>|]), [|Expression.Constant(ParameterFormat) :> Expression; getColumn :> Expression|])
        { Type = columnType; ColumnName = getColumnName column; Append = Expression.Lambda<Action<StringBuilder, obj>>(body, true, [|target; value|]).Compile() }

    [<CompiledName("GetFormatter")>]
    let getFormatter(t:Type) = 
        let publicInstance = BindingFlags.Public + BindingFlags.Instance 
        match (t.GetFields(publicInstance), t.GetProperties(publicInstance)) with
        | ([|column|], [||]) -> singleColumnFormatter(t, column, column.FieldType)
        | ([||], [|column|]) -> singleColumnFormatter(t, column, column.PropertyType)
        | x -> raise(NotSupportedException("Only single column tables supported"))

type XmlParameter<'a>(parameterName) =
    let parameters = ParameterCollection()
    let tableName = XmlParameter.getTableName typeof<'a>
    let formatter = XmlParameter.getFormatter typeof<'a>
    let columnType = formatter.Type
    let columnName = formatter.ColumnName

    interface System.Collections.IEnumerable with
        member this.GetEnumerator() = parameters.GetEnumerator() :> System.Collections.IEnumerator

    member this.Add(value : 'a) = parameters.Add(value)
    member this.AddRange(values : 'a seq) = parameters.AddRange(values)

    member this.TempTableDefinition() = 
        let columnType = 
            match columnType with
            | x when x = typeof<String> -> "varchar(max)"
            | x when x = typeof<int> -> "int"
            | x when x = typeof<Guid> -> "uniqueidentifier"
            | _ -> raise(NotSupportedException("Unsupported column type: " + columnType.Name))
        String.Format("select [{0}] = x.value('.', '{4}') into {1} from {2}.nodes('/{3}') _(x)\n", columnName, tableName, parameterName, XmlParameter.ParameterTagName, columnType)

    member this.Inject(command : IDbCommand) =
        command.CommandText <- this.TempTableDefinition() + command.CommandText
        let p = command.CreateParameter()
        p.ParameterName <- parameterName
        p.DbType <- DbType.Xml
        p.Value <- this.GetValue()
        command.Parameters.Add(p) |> ignore

    member private this.GetValue() = 
        let result = StringBuilder()
        for item in parameters do
            formatter.Append.Invoke(result, item)
        result.ToString()
    
    member this.ToSqlParameter() = SqlParameter(ParameterName = parameterName, DbType = DbType.Xml, Value = this.GetValue())
