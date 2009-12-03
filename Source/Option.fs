namespace Xlnt.Stuff

module Option =
    [<CompiledName("Maybe")>]
    let maybe = function
        | null -> None
        | x -> Some(x)
        
    [<CompiledName("GetValueOrDefault")>]
    let getOrDefault def = function
        | Some(x) -> x
        | None -> def        
