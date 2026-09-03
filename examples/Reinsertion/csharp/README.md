# Reinsertion in C#

This example uses the `GeneticAlgorithms` library from C# to compare the three **reinsertion strategies** in the `Reinsertion` module - `pure`, `elitist`, and `uniform` - by running the exact same problem three times, once per strategy, and comparing the results side by side.

This mirrors the F# `Reinsertion` example; see [its README](../fsharp/README.md) for the full explanation of reinsertion and why this example uses a large OneMax-style problem instead of a small, relatable one. This README focuses on what's different about the C# version.

## The Problem

A OneMax-style problem: maximize the number of `1`s in a 500-gene binary chromosome.

## Purpose

Unlike the other C# examples, which call `GeneticAlgorithm.Run` with just a population size or a handful of delegates, this example needs to plug in a custom **reinsertion** strategy and observe **per-generation progress** - neither of which the library's C# facade (`GeneticAlgorithm.CreateOptions`) exposed before this example was added. Building it required extending `GeneticAlgorithm.CreateOptions` with two new overload parameters:

* `reinsertionFn` - a `Func<Chromosome<T>[], Chromosome<T>[], Chromosome<T>[], Chromosome<T>[]>` for `Options.ReinsertionFn`.
* `onGeneration` - an `Action<Chromosome<T>, int>` for `Options.OnGeneration`, used here to record the best fitness at every generation.

Both mirror how `selectionFn`, `crossoverFn`, and `mutationFn` were already exposed, so any C# consumer - not just this example - can now configure every part of the algorithm without touching F#-specific types.

Passing the library's own reinsertion functions in from C# works without any extra glue code:

```csharp
("pure", Reinsertion.pure),
("elitist", (parents, offspring, leftover) => Reinsertion.elitist(survivalRate, parents, offspring, leftover)),
("uniform", (parents, offspring, leftover) => Reinsertion.uniform(survivalRate, parents, offspring, leftover)),
```

`Reinsertion.pure` converts directly to a `Func<>` via a method group conversion, the same way `Selection.elite` already does elsewhere in the C# examples. `elitist` and `uniform` take an extra `survivalRate` argument ahead of the three reinsertion parameters, so they need a short lambda to fix that argument - but no special interop code beyond that.

## Running the Example

From the repository root:

```powershell
dotnet run --project examples/Reinsertion/csharp
```

## Expected Output

```text
Maximum possible fitness: 500 (all 500 genes set to 1)

Best fitness by generation (sampled every 30 generations):
Generation |     pure |  elitist |  uniform
         0 |    272.0 |    281.0 |    272.0
        30 |    312.0 |    377.0 |    345.0
        60 |    315.0 |    446.0 |    407.0
        ...
       300 |    325.0 |    500.0 |    500.0

Final results:
pure     fitness: 325.0 / 500 ( 65.0% of maximum)
elitist  fitness: 500.0 / 500 (100.0% of maximum)
uniform  fitness: 500.0 / 500 (100.0% of maximum)
```

As with the F# version, `elitist` and `uniform` reliably reach the perfect score and hold it, while `pure` plateaus well short of the maximum. Because the algorithm is randomized, exact values vary between runs, but that shape is consistent. (Depending on your machine's locale, decimal numbers may print with a comma instead of a period - this example doesn't force a specific culture, matching the other C# examples in this library.)

## Related Projects

This example is part of the GeneticAlgorithms library. It's the C# counterpart to the [`Reinsertion`](../fsharp/README.md) F# example, and it's what motivated adding `reinsertionFn` and `onGeneration` to `GeneticAlgorithm.CreateOptions`.
