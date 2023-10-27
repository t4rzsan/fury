namespace Fury.Buffer

open System
open System.Threading

module FixedSizeArrayPool =
    type  FixedSizeArrayPool<'T>(arraySize: int, capacity: int) =
        [<DefaultValue>] static val mutable private shared : FixedSizeArrayPool<'T>
        static let mutable count = 0

        let mutable currentIndex: int = 0
        let pool : 'T[] [] = Array.zeroCreate capacity

        do
            if capacity <= 0 then
                raise (ArgumentException("Capacity must be greater than zero."))

            if arraySize <= 0 then
                raise (ArgumentException("Array size must be greater than zero."))

            currentIndex <- capacity - 1

            for i in 0 .. capacity - 1 do
                pool[i] <- Array.zeroCreate arraySize

        static member Shared
            with get () = FixedSizeArrayPool.shared
            
        static member CreateShared (arraySize: int, capacity: int) =
            FixedSizeArrayPool.shared <- FixedSizeArrayPool(arraySize, capacity)
        
        member this.Rent() =
            // If we have reached the end of the pool, fail with an exception.
            // Reading integers is atomic (I think).
            if currentIndex = -1 then
                raise (InvalidOperationException("Pool is empty."))

            let index = 1 + Interlocked.Decrement(&currentIndex)

            let array = pool[index]
            pool[index] <- null
            array

        member this.Return clear (array: 'T[]) =
            if isNull(array) || not (array.Length = arraySize) then
                raise (ArgumentException($"Array must be of length {arraySize}."))

            let index = Interlocked.Increment(&currentIndex)
            if index = capacity then
                Interlocked.Decrement(&currentIndex) |> ignore
                raise (InvalidOperationException("Pool is full."))

            if clear then
                let span = Span(array)
                span.Clear()

            pool[index] <- array
