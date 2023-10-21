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
        let pool = FixedSizeArrayPool<int>(5, 1)
        let array = pool.Rent()
        array[2] <- 42
        
        pool.Return true array

        // This is for testing purposes.  You should never
        // access the array after returning it.
        Assert.Equal(0, array[2])

    [<Fact>]
    let ``Resizing`` () =
        let pool = FixedSizeArrayPool<int>(5, 1)
        let array1 = pool.Rent()
        // Force resize
        let array2 = pool.Rent()
        // Force resize
        let array3 = pool.Rent()
        
        array1[2] <- 1
        array2[2] <- 2
        array3[2] <- 3
    
        pool.Return false array1
        pool.Return false array2
        pool.Return false array3

        // This is for testing purposes.  You should never
        // access the array after returning it.
        Assert.Equal(1, array1[2])
        Assert.Equal(2, array2[2])
        Assert.Equal(3, array3[2])

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
