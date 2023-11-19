namespace Fury.Test

open Fury.Buffer.FixedSizeArrayPool

open Xunit
open System

module Tests = 
    [<Fact>]
    let ``Rent and return without clearing`` () =
        let pool = FixedSizeArrayPool<int>(5, 1)
        let array = pool.Rent()
        array[2] <- 42
        
        pool.Return false array 

        Assert.Equal(42, array[2])

    [<Fact>]
    let ``Rent and return with clearing`` () =
        let pool = FixedSizeArrayPool<int>(arraySize = 5, capacity = 1)
        let array = pool.Rent()
        array[2] <- 42
        
        pool.Return true array

        // This is for testing purposes.  You should never
        // access the array after returning it.
        Assert.Equal(0, array[2])

    [<Fact>]
    let ``Return array of wrong size should throw`` () =
        let pool = FixedSizeArrayPool<int>(5, 1)

        let _ = pool.Rent()
        let array = Array.zeroCreate 4

        Assert.Throws<ArgumentException>(fun () -> pool.Return false array)

    [<Fact>]
    let ``Return array to full pool should throw`` () =
        let pool = FixedSizeArrayPool<int>(5, 1)

        let array = Array.zeroCreate 5

        Assert.Throws<InvalidOperationException>(fun () -> pool.Return false array)

    [<Fact>]
    let ``Non-positive array size throws`` () =
        Assert.Throws<ArgumentException>(fun () -> FixedSizeArrayPool<int>(0, 1) |> ignore)

    [<Fact>]
    let ``Non-positive capacity throws`` () =
        Assert.Throws<ArgumentException>(fun () -> FixedSizeArrayPool<int>(1, 0) |> ignore)
