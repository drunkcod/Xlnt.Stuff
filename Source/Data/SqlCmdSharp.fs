namespace Xlnt.Data

open System.IO
open System.Text
open System.Text.RegularExpressions

module SqlCmdSharp =
    let private endOfBatchPattern = Regex("^\s*GO\s*(\d+)?(--.*)?$", RegexOptions.Compiled + RegexOptions.IgnoreCase)
    let IsEndOfBatch line = endOfBatchPattern.IsMatch(line)

    [<CompiledName("ReadQueryBatches")>]
    let readQueryBatches (reader:TextReader) onBatchReady =
        let batchReady = string >> (function
            | "" -> ()
            | batch -> onBatchReady batch)
        let rec next (batch:StringBuilder) =
            match reader.ReadLine() with
            | null -> batchReady batch
            | line when IsEndOfBatch(line) ->
                batchReady batch
                next (StringBuilder())
            | line -> batch.AppendLine(line) |> next
            
        next (StringBuilder())
