# Mutation in C#

This example uses the `GeneticAlgorithms` library from C# to compare two **mutation strategies** in the `Mutation` module - `scramble` and `scrambleSlice` - by running the exact same problem twice, once per strategy, and comparing the results side by side.

This mirrors the F# `Mutation` example; see [its README](../fsharp/README.md) for the full explanation of the scheduling problem and why an order-dependent fitness function is essential for comparing mutation strategies. This README focuses on what's different about the C# version.

## The Problem

Single-machine weighted tardiness scheduling: given a fixed set of 30 jobs, each with a processing time, a due date, and an importance weight, find the processing order that minimizes total weighted tardiness.

## Passing `scramble` and `scrambleSlice` from C#

```csharp
var strategies = new (string Name, Func<Random, Chromosome<int>, Chromosome<int>> MutationFn)[]
{
    ("scramble", Mutation.scramble),
    ("scrambleSlice", (rng, chromosome) => Mutation.scrambleSlice(scrambleSliceWindow, rng, chromosome)),
};
```

`Mutation.scramble` converts directly to a `Func<>` via a method group conversion, the same way `Selection.elite`/`Reinsertion.pure` already do elsewhere in the C# examples. `scrambleSlice` takes an extra `n` (window size) argument ahead of the two mutation parameters, so it needs a short lambda to fix that argument - no special interop code beyond that.

## Running the Example

From the repository root:

```powershell
dotnet run --project examples/Mutation/csharp
```

## Expected Output

```text
Minimizing total weighted tardiness across 30 jobs over 300 generations...

Best (lowest) total weighted tardiness by generation (sampled every 30 generations):
Generation |   scramble | scrambleSlice
         0 |     4516,0 |        3558,0
        30 |     1767,0 |        1538,0
        60 |     1750,0 |        1255,0
        90 |     1750,0 |        1176,0
       120 |     1750,0 |        1127,0
       ...
       300 |     1750,0 |        1120,0

Final results:
scramble      total weighted tardiness: 1750,0
scrambleSlice total weighted tardiness: 1120,0
```

As with the F# version, `scramble` improves quickly at first and then plateaus, while `scrambleSlice` keeps finding small, incremental improvements well past that point. Because the algorithm is randomized, exact values vary between runs, but that shape is consistent. (Depending on your machine's locale, decimal numbers may print with a comma instead of a period, as in the sample above - this example doesn't force a specific culture, matching the other C# examples in this library.)

## Related Projects

This example is part of the GeneticAlgorithms library. It's the C# counterpart to the [`Mutation`](../fsharp/README.md) F# example, and follows the same side-by-side comparison format as the [`Reinsertion`](../../Reinsertion/csharp/README.md) C# example.
