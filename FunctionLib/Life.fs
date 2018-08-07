﻿namespace FunctionLib

module Life =
    // wrap values for edge wrapping
    let rec wrapClamp i max =
        match i with
        | v when v < 0 -> wrapClamp (i+max) max
        | v when v >= max -> wrapClamp (i-max) max
        | _ -> i

    // convert bool for indexed list to numeric value
    let getCellValue i (l :array<bool>) =
        match l.[(wrapClamp i l.Length)] with
        | true -> 1
        | false -> 0

    // count number of living neighbors
    let getNeighbors i list stride =
        List.fold (fun count st -> count + (getCellValue (st+i) list)) 0 stride    
    
    // conway's game of life rules:
    // 3 neighbors always live and 2 always keep their state
    // everything else dies
    let isAlive current neighbors =
        match neighbors with
        | 2 -> current
        | 3 -> true
        | _ -> false
    
    // get cell indexes for a certain stride
    let getNeighborOffsets stride = [1;-1;-stride;-stride+1;-stride-1;stride;stride+1;stride-1]

    // process the list with our rules
    let lifeGameTick (cells : array<bool>) stride = 
        let offsets = getNeighborOffsets stride
        [for i in 0 .. cells.Length-1 do
            yield isAlive cells.[i] (getNeighbors i cells offsets)]