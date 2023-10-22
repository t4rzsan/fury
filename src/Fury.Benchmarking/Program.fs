open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running

type ArrayClearBenchmark() = 
    let array : int[] = Array.zeroCreate 1_000_000

    [<ParamsSource("ValuesForArray")>]
    member val Array = array with get, set

    member this.ValuesForArray : int [][] = 
        [| Array.zeroCreate 1_000_000; Array.zeroCreate 100 |]  

    [<Benchmark(Baseline = true)>]
    member this.LargeSystemArrayClear() = 
        System.Array.Clear(this.Array)

    [<Benchmark()>]
    member this.LargeIterate() = 
        for i in 0 .. this.Array.Length - 1 do
            array.[i] <- 0

    [<Benchmark()>]
    member this.LargeSpanClear() = 
        let span = System.Span(this.Array)
        span.Clear()


[<EntryPoint>]
let main argv =
    BenchmarkRunner.Run<ArrayClearBenchmark>() |> ignore
    0