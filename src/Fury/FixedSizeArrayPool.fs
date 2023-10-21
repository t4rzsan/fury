namespace Fury.Buffer

open System

module FixedSizeArrayPool =
    type  FixedSizeArrayPool<'T>(arraySize: int, capacity: int) =
        let mutable currentIndex = capacity - 1
        let mutable size = capacity
        let mutable pool : 'T[] [] = Array.zeroCreate size

        do
            for i in 0 .. size - 1 do
                pool.[i] <- Array.zeroCreate arraySize
    
        member this.Rent() =
            // If we have reached the end of the pool, resize the pool
            // and initialize it with empty arrays from the bottom, so
            // we still have room for returned arrays at the top.
            if currentIndex = 0 then
                let oldSize = size
                let size = oldSize + capacity
                pool <- Array.zeroCreate size

                for i in 0 .. capacity - 1 do
                    pool.[i] <- Array.zeroCreate arraySize

                currentIndex <- capacity - 1

            let index =  currentIndex
            currentIndex <- currentIndex - 1

            let array = pool[index]
            pool[index] <- null
            array

        member this.Return (array: 'T[]) clear =
            if isNull(array) || not (array.Length = arraySize) then
                failwith $"Array must be of length {arraySize}."

            if clear then
                let span = Span(array)
                span.Clear()

            currentIndex <- currentIndex + 1
            pool.[currentIndex] <- array
