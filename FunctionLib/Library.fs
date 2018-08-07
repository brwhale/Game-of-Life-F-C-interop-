namespace FunctionLib

module Main =
    // various functions to play with stuff
    let mult = function
        | Some n -> (fun x -> Some(n*x))
        | None -> (fun x -> None)

    let rec fact = function
        | Some 0 -> Some(1)
        | None -> None
        | Some n when n < 0 -> None
        | Some n -> mult (fact (Some(n - 1))) n

    let string = function
        | Some a -> a.ToString()
        | None -> ""

    let factorial x =
        string (fact (Some(x)))

    let printList l =
        for a in l do 
            printfn "%s " (string a)

    let invertList l =
        [for a in l do yield (not a)]

    let main fnc =
        let x = 5
        let y = 1
        let z = x - y
        let q = y - x
        fnc (sprintf "x: %i, y: %i, z: %i, q: %i, z!: %s, q!: %s\n" x y z q (factorial z) (factorial q))