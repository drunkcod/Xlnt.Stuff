namespace Xlnt

open System.Diagnostics

module Timed =
    let action f report =
        let stopwatch = Stopwatch.StartNew()
        let r = f()
        stopwatch.Stop()
        report(r, stopwatch.Elapsed)
        r
