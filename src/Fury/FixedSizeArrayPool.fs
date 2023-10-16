namespace Fury.Buffer

open System

module FixedSizeArrayPool =
    type  FixedSizeArrayPool<'T>(arraySize: int, capacity: int) =
        let mutable pool : 'T[] [] = Array.zeroCreate capacity
        let mutable currentIndex = capacity - 1

        do
            for i in 0 .. capacity - 1 do
                pool.[i] <- Array.zeroCreate arraySize
    
        member this.Rent() =
            if currentIndex = 0 then
                raise (IndexOutOfRangeException("Capacity has been reached."))

            let index =  currentIndex
            currentIndex <- currentIndex - 1
            pool.[index]

        member this.TryRent() =
            if currentIndex = 0 then
                None
            else
                let index =  currentIndex
                currentIndex <- currentIndex - 1
                Some pool.[index]
