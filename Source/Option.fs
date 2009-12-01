namespace Xlnt.Stuff

module Option =
    [<CompiledName("Maybe")>]
    let maybe = function
        | null -> None
        | x -> Some(x)
