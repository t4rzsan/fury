# Fury
A small library that contains a fixed size array pool.  Use the array pool to avoid garbage collection of large arrays.  This is a supplement to .NET's built-in [`ArrayPool`](https://learn.microsoft.com/en-us/dotnet/api/system.buffers.arraypool-1?view=net-6.0) which is not fixed size.  `ArrayPool` only guarantees that the arrays are at least of the size requested.  Fury's `FixedSizeArrayPool` always returns arrays of the same size.

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

## Thread safety
`FixedSizeArrayPool` is **not** thread safe.
