namespace FunctionLib

module Life =
    // convert bool to numeric value
    let getIntDex i (l :list<bool>) =
        if i > 0 && i < l.Length && l.[i] then
            1
        else
            0
    
    // there's got to be a better way to do this
    let getNeighbors i list stride =
        (getIntDex (i-1) list) + (getIntDex (i+1) list) + (getIntDex (i-stride+1) list) + 
        (getIntDex (i-stride) list) + (getIntDex (i-stride-1) list) + (getIntDex (i+stride+1) list) + 
        (getIntDex (i+stride) list) + (getIntDex (i+stride-1) list)      
    
    // conway's game of life rules:
    // more than 3 or less than 2 neighbors die,
    // 3 always live and 2 always keep their state
    let isAlive current neighbors =
        if neighbors < 2 || neighbors > 3 then
            false
        else if neighbors = 3 then
            true
        else
            current

    // process the list with our rules
    let lifeGameTick (cells : list<bool>) stride = 
        [for i in 0 .. cells.Length-1 do
            yield isAlive cells.[i] (getNeighbors i cells stride)]

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