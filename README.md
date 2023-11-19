# Fury
A small library that contains a fixed size array pool.  Use the array pool to avoid garbage collection of large arrays.  This is a supplement to .NET's built-in [`ArrayPool`](https://learn.microsoft.com/en-us/dotnet/api/system.buffers.arraypool-1?view=net-6.0) which is not fixed size.  `ArrayPool` only guarantees that the arrays are at least of the size requested.  Fury's `FixedSizeArrayPool` always returns arrays of the same size.

## Installation
Currently, there is no Nuget package or anything to install.  If you want to use `FixedArrayPool`, just grab the code from the `src` folder.

## Usage
You create a new `FixedSizeArrayPool` by specifying the array length and the initial capacity.  The pool will initially create all arrays, i.e. they are **not** lazily created.
```fsharp
open Fury.Buffer.FixedSizeArrayPool

// Create a pool to initially hold 100 int arrays of length 2000.
let pool = FixedSizeArrayPool<int>(arraySize = 2000, capacity = 100)
```
Now get an array from the pool:
```fsharp
let array = pool.Rent ()
```
After use, you must return the array to the pool for re-use.  Otherwise, the point of the pool is moot.  Upon returning you have the choice of clearing the array or leaving it as it is with data.  Clearing hurts performance.
```fsharp
let clear = false
pool.Return clear array
```
After you have returned an array to the pool, you must never use it again outside the pool.  It is now owned by the pool.

### Why does `FixedSizeArrayPool` not have a property like [`ArrayPool.Shared`](https://learn.microsoft.com/en-us/dotnet/api/system.buffers.arraypool-1.shared?view=net-7.0)?
It would not make any sense to add a `Shared` static property to `FixedSizeArrayPool` because it would only be able to hold a specific size arrays.  Instead you can roll your own global registry like so:
```fsharp
type FixedSizeArrayRegistry() =
    static let shared100 = FixedSizeArrayPool(100, 500)
    static let shared200 = FixedSizeArrayPool(200, 500)

    static member Shared100
        with get () = shared100

    static member Shared200
        with get () = shared200
```

Then you can use it like so:
```fsharp
let pooledArrayOfLength100 = FixedSizeArrayRegistry.Shared100.Rent ()
```

### Exceptions
`FixedSizeArrayPool` will throw exceptions if you rent more arrays than the `capacity`, and if you try to return more than `capacity` arrays.  Also, you cannot return arrays of incorrect length.

## Thread safety
`FixedSizeArrayPool` is thread safe.

## Performance
### Clearing returned arrays

I decided to use `Span.Clear` to clear returned arrays.  The project `Fury.Benchmarking` contains a benchmark test that comparing clearing arrays with either `Array.Clear`, iteration and `Span.Clear`.  Iteration is clearly the slower one, and `Span.Clear` seems to have a small edge over `Array.Clear`.  So I went with `Span.Clear`.

| Method                | Array          | Mean          | Error      | StdDev     | Ratio |
|---------------------- |--------------- |--------------:|-----------:|-----------:|------:|
| SystemArrayClear | Int32[1000000] |  78,950.00 ns | 257.829 ns | 241.174 ns | 1.000 |
| Iterate          | Int32[1000000] | 239,555.79 ns | 660.288 ns | 617.633 ns | 3.034 |
| SpanClear        | Int32[1000000] |  77,947.21 ns | 404.416 ns | 378.291 ns | 0.987 |
| SystemArrayClear | Int32[100]     |      13.09 ns |   0.046 ns |   0.043 ns | 0.000 |
| Iterate          | Int32[100]     |      29.62 ns |   0.071 ns |   0.063 ns | 0.000 |
| SpanClear        | Int32[100]     |      12.69 ns |   0.019 ns |   0.018 ns | 0.000 |
