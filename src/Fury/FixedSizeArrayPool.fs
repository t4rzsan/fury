namespace Fury.Buffer

open System

module FixedSizeArrayPool =
    type  FixedSizeArrayPool<'T>(arraySize: int, capacity: int) =
        let mutable currentIndex: int = 0
        let mutable size = 0
        let mutable pool : 'T[] [] = [||]

        do
            if capacity <= 0 then
                raise (ArgumentException("Capacity must be greater than zero."))

            if arraySize <= 0 then
                raise (ArgumentException("Array size must be greater than zero."))

            currentIndex <- capacity - 1
            size <- capacity
            pool <- Array.zeroCreate size

            for i in 0 .. size - 1 do
                pool[i] <- Array.zeroCreate arraySize
    
        member this.Rent() =
            // If we have reached the end of the pool, resize the pool
            // and initialize it with empty arrays from the bottom, so
            // we still have room for returned arrays at the top.
            if currentIndex = -1 then
                let oldSize = size
                size <- oldSize + capacity
                pool <- Array.zeroCreate size

                // Initialize with new empty arrays from the bottom
                for i in 0 .. capacity - 1 do
                    pool[i] <- Array.zeroCreate arraySize

                currentIndex <- capacity - 1

            let index = currentIndex
            currentIndex <- currentIndex - 1

            let array = pool[index]
            pool[index] <- null
            array

        member this.Return clear (array: 'T[]) =
            if isNull(array) || not (array.Length = arraySize) then
                raise (ArgumentException($"Array must be of length {arraySize}."))

            if currentIndex = size - 1 then
                raise (InvalidOperationException("Pool is full."))

            if clear then
                let span = Span(array)
                span.Clear()

            currentIndex <- currentIndex + 1
            pool[currentIndex] <- array


