namespace FunctionLib

module Life =
    // wrap values for edge wrapping
    let rec wrapClamp n max =
        match n with
        | v when v < 0 -> wrapClamp (n+max) max
        | v when v >= max -> wrapClamp (n-max) max
        | _ -> n

    // convert bool for indexed list to numeric value
    let getCellValue index (l :array<bool>) =
        match l.[(wrapClamp index l.Length)] with
        | true -> 1
        | false -> 0

    // count number of living neighbors
    let getNeighbors index list offsets =
        List.fold (fun count offset -> count + (getCellValue (offset+index) list)) 0 offsets    
    
    // conway's game of life rules:
    // 3 neighbors always live and 2 always keep their state
    // everything else dies
    let isAlive current neighbors =
        match neighbors with
        | 2 -> current
        | 3 -> true
        | _ -> false

    let isAliveAltRules current neighbors =
        match neighbors with
        | 2 | 7 -> current
        | 3 | 6 -> true
        | _ -> false
    
    // get cell indexes for a certain stride
    let getNeighborOffsets stride = [1;-1;-stride;-stride+1;-stride-1;stride;stride+1;stride-1]

    // process the list with our rules
    let lifeGameTick (cells : array<bool>) stride = 
        let offsets = getNeighborOffsets stride
        [for i in 0 .. cells.Length-1 do
            let neighbors = getNeighbors i cells offsets
            yield (neighbors, isAlive cells.[i] neighbors)]

    // process the list with different rules
    let notlifeGameTick (cells : array<bool>) stride = 
        let offsets = getNeighborOffsets stride
        [for i in 0 .. cells.Length-1 do
            let neighbors = getNeighbors i cells offsets
            yield (neighbors, isAliveAltRules cells.[i] neighbors)]