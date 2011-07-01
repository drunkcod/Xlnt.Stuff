namespace Xlnt.Data

open System
open System.Data.Common
open Xlnt

type ProfiledDataReader(inner:DbDataReader) = 
    inherit DbDataReader() with
        let beginRow = new Event<_>()
        let endRow = new Event<_>()

        override this.Depth
            with get() = inner.Depth

        override this.FieldCount
            with get() = inner.FieldCount

        override this.HasRows
            with get() = inner.HasRows

        override this.IsClosed
            with get() = inner.IsClosed

        override this.RecordsAffected 
            with get() = inner.RecordsAffected

        override this.Item
            with get(ordinal:int) = inner.[ordinal]

        override this.Item
            with get(name:string) = inner.[name]

        override this.Close() = inner.Close()

        override this.GetDataTypeName(ordinal) = inner.GetDataTypeName(ordinal)

        override this.GetEnumerator() = inner.GetEnumerator()

        override this.GetFieldType(ordinal) = inner.GetFieldType(ordinal)

        override this.GetName(ordinal) = inner.GetName(ordinal)

        override this.GetOrdinal(name) = inner.GetOrdinal(name)

        override this.GetSchemaTable() = inner.GetSchemaTable()

        override this.GetBoolean(ordinal) = inner.GetBoolean(ordinal)

        override this.GetByte(ordinal) = inner.GetByte(ordinal)

        override this.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length) = inner.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length)

        override this.GetChar(ordinal) = inner.GetChar(ordinal)

        override this.GetChars(ordinal, dataOffset, buffer, bufferOffset, length) = inner.GetChars(ordinal, dataOffset, buffer, bufferOffset, length)

        override this.GetDateTime(ordinal) = inner.GetDateTime(ordinal)

        override this.GetDecimal(ordinal) = inner.GetDecimal(ordinal)

        override this.GetDouble(ordinal) = inner.GetDouble(ordinal)

        override this.GetFloat(ordinal) = inner.GetFloat(ordinal)

        override this.GetGuid(ordinal) = inner.GetGuid(ordinal)

        override this.GetInt16(ordinal) = inner.GetInt16(ordinal)

        override this.GetInt32(ordinal) = inner.GetInt32(ordinal)

        override this.GetInt64(ordinal) = inner.GetInt64(ordinal)

        override this.GetString(ordinal) = inner.GetString(ordinal)

        override this.GetValue(ordinal) = inner.GetValue(ordinal)

        override this.GetValues(values) = inner.GetValues(values)

        override this.IsDBNull(ordinal) = inner.IsDBNull(ordinal)

        override this.NextResult() = inner.NextResult()

        override this.Read() =
            beginRow.Trigger(this)            
            Timed.action inner.Read (fun (success, elapsed) -> if success then endRow.Trigger((this, elapsed)))

        interface IDisposable with
            member this.Dispose() = 
                inner.Dispose()
                base.Dispose()

        member this.BeginRow = beginRow.Publish
        member this.EndRow = endRow.Publish

