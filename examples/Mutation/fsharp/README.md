# Mutation

This project compares two of the `GeneticAlgorithms` library's **mutation strategies** - `Mutation.scramble` and `Mutation.scrambleSlice` - by running the exact same problem twice, once per strategy, and comparing the results side by side.

## The Problem

Single-machine weighted tardiness scheduling: given a fixed set of 30 jobs, each with a processing time, a due date, and an importance weight, find the processing *order* that minimizes total weighted tardiness (jobs run back to back, with no idle time between them).

This is a genuinely **order-dependent** problem, and that's essential here. `scramble` and `scrambleSlice` only differ in *how* they reorder a chromosome's genes - both preserve the exact same multiset of values. On an order-*insensitive* fitness function (like `OneMaxProblem`'s gene sum, used by the `Reinsertion` example), reordering genes changes nothing about the fitness at all, so the two strategies would be indistinguishable no matter how differently they mutate. A schedule's cost, by contrast, depends entirely on the order jobs run in - reordering the same set of jobs changes which ones finish late, and by how much.

The job data itself (processing times, due dates, weights) is a fixed problem instance, generated once from its own dedicated, separately-seeded `System.Random(42)` - not part of what's randomized every run. Only the search itself (initial population, selection, crossover, mutation) draws from `Options.Random`.

## What Is Mutation, Here?

A chromosome's genes are a permutation of job indices - the order jobs run in. Both strategies compared here only ever *reorder* a chromosome's existing genes; neither can introduce a job index that wasn't already present (that's what makes them safe for a permutation genotype in the first place - see `Mutation.randomReset` for a strategy that can't make this same guarantee).

* **`scramble`** - shuffles every gene in the chromosome into a new order, every time it's applied.
* **`scrambleSlice`** - shuffles the genes inside a small, random contiguous window (5 jobs here, out of 30), leaving everything outside that window untouched.

## How This Example Is Configured

```fsharp
let scrambleSliceWindow = 5

let baseOptions: Options<int> =
    { Options.create 100 with
        CrossoverFn = Crossover.orderOneCrossover }
```

`CrossoverFn` is `Crossover.orderOneCrossover` rather than the library's default `singlePoint`: genes here are a permutation (every job scheduled exactly once), and a single-point cut would generally produce a schedule with one job missing and another repeated. `scrambleSliceWindow` is deliberately small relative to `numberOfJobs` (30) - the whole point of this example is comparing `scrambleSlice` against `scramble`'s full-chromosome reorder, so a window that's a small, local fraction of the chromosome is what makes it "less disruptive" in the first place; a window close to 30 would just behave like `scramble`.

## How It Works

1. Run the scheduling problem for 300 generations, once with each mutation strategy plugged into `Options.MutationFn` - everything else (population size, selection, crossover, reinsertion) stays identical across the two runs.
2. Record the best (lowest) total weighted tardiness seen at every generation via `Options.Probe`, so the two runs can be compared at matching points in time afterward.
3. Print a table of best cost at every 30th generation, side by side for both strategies.
4. Print each strategy's final total weighted tardiness.

## Running the Example

From the repository root:

```powershell
dotnet run --project examples/Mutation/fsharp
```

## Expected Output

```text
Minimizing total weighted tardiness across 30 jobs over 300 generations...

Best (lowest) total weighted tardiness by generation (sampled every 30 generations):
Generation |   scramble | scrambleSlice
         0 |     4476.0 |        4231.0
        30 |     1315.0 |        1561.0
        60 |     1249.0 |        1272.0
        90 |     1249.0 |        1199.0
       120 |     1249.0 |        1142.0
       150 |     1249.0 |        1118.0
       180 |     1249.0 |        1118.0
       210 |     1249.0 |        1115.0
       240 |     1249.0 |        1113.0
       270 |     1249.0 |        1113.0
       300 |     1249.0 |        1110.0

Final results:
scramble      total weighted tardiness: 1249.0
scrambleSlice total weighted tardiness: 1110.0
```

Across repeated runs, the same pattern shows up reliably: `scramble` improves quickly at first, then plateaus early and stops making further progress - it's re-shuffling the *entire* schedule every time, so an improvement in one part of the order is just as likely to be wiped out by disruption elsewhere. `scrambleSlice` keeps finding small, incremental improvements well past that point, since disturbing a handful of jobs at a time is far less likely to destroy an already-good ordering elsewhere in the schedule. Because the algorithm is randomized, the exact costs and generation counts vary between runs, but the *shape* of the comparison - `scramble` stalling noticeably earlier than `scrambleSlice` - is consistent.

## Why This Example?

The other examples in this library each demonstrate a single configuration end to end. This one instead holds every setting fixed except one - the mutation strategy - specifically to make that one setting's effect visible, the same way the `Reinsertion` example does for reinsertion strategies. It's a useful companion to the `Mutation` module's own doc comments: where those describe `scrambleSlice` as "a less disruptive mutation for larger chromosomes," this example shows what that difference actually looks like over a real run.

## Related Projects

This example is part of the GeneticAlgorithms library and follows the same side-by-side comparison format as the [`Reinsertion`](../../Reinsertion/fsharp/README.md) example, applied to mutation strategies instead of reinsertion strategies. Like [`NQueens`](../../NQueens/fsharp/README.md) and [`TravelingSalesman`](../../TravelingSalesman/fsharp/README.md), its genotype is permutation-encoded, so it also uses `Crossover.orderOneCrossover`.
