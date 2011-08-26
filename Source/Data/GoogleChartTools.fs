namespace Xlnt.Data.GoogleChartTools

open System
open System.Collections.Generic
open System.Data
open System.Runtime.Serialization

[<DataContract>]
type GoogleChartColumn =
    struct 
        [<DataMember(Name="label")>] val mutable Label : string 
        [<DataMember(Name="type")>] val mutable Type : string

        static member private MapColumnType = function
            | x when x = typeof<int> -> "number"
            | x when x = typeof<string> -> "string"
            | x when x = typeof<DateTime> -> "date"
            | x -> raise(NotSupportedException(x.FullName))

        new(label, t) = { Label = label; Type = GoogleChartColumn.MapColumnType t }
    end

[<DataContract>]
type GoogleChartValue = 
    struct
        [<DataMember(Name="v")>] val mutable Value : obj
    end

[<DataContract>]
type GoogleChartRow =
    struct
        [<DataMember(Name="c")>] val mutable Values : GoogleChartValue seq
    end

[<DataContract>]
type GoogleChartData =
    struct
        [<DataMember(Name="cols")>] val mutable Columns : GoogleChartColumn seq
        [<DataMember(Name="rows")>] val mutable Rows : GoogleChartRow seq
        
        static member From(reader : IDataReader) = 
            let columns = List()
            for i = 0 to reader.FieldCount - 1 do
                columns.Add(GoogleChartColumn(reader.GetName(i), reader.GetFieldType(i)))
            
            let rows = List()
            while reader.Read() do 
                let row = List()
                for i = 0 to reader.FieldCount - 1 do
                    row.Add(GoogleChartValue(Value = reader.GetValue(i)))
                rows.Add(GoogleChartRow(Values = row))

            GoogleChartData(Columns = columns, Rows = rows)
    end
