# Reinsertion

This project demonstrates the three **reinsertion strategies** in the `GeneticAlgorithms` library's `Reinsertion` module - `pure`, `elitist`, and `uniform` - by running the exact same problem three times, once per strategy, and comparing the results side by side.

## The Problem

A OneMax-style problem: maximize the number of `1`s in a 500-gene binary chromosome. This is deliberately simple and large. The point of this example is to compare reinsertion strategies, not to showcase an interesting problem, so a huge, unconstrained search space is the right choice here: it makes how well a run explores and *preserves* diversity the only thing that determines the outcome - which is exactly what reinsertion controls.

A smaller search space (this example originally reused the `Schedule` example's 10-gene scheduling problem) doesn't work well for this: with only 1,024 possible chromosomes, even a badly shrunk population can stumble onto a near-optimal answer by luck, which hides the differences between strategies rather than showing them.

## What Is Reinsertion?

At the end of every generation, a genetic algorithm has three groups of chromosomes to decide what to do with:

* **`parents`** - the chromosomes selected to become crossover parents this generation.
* **`offspring`** - this generation's crossover children, plus any mutants.
* **`leftover`** - the chromosomes that weren't selected as parents.

A reinsertion strategy decides how these three groups combine into the population that carries into the next generation. This matters because `offspring` is not guaranteed to be the same size as the population it's replacing - what a reinsertion strategy does with `leftover` in particular determines whether the population shrinks, grows, or stays roughly the same size over time.

## The Three Strategies

* **`pure`** - replaces the population outright with `offspring`, discarding `parents` and `leftover` entirely. The simplest possible strategy, but with a real cost: since nothing backfills what was discarded, the population drifts in size every generation.
* **`elitist`** - pools `parents` and `leftover` back together and carries over the *fittest* fraction of them (`survivalRate`) alongside `offspring`. This preserves good genes that crossover or mutation might otherwise have lost.
* **`uniform`** - the same idea as `elitist`, but the survivors are chosen *at random* rather than by fitness.

## How This Example Is Configured

```fsharp
let survivalRate = 0.15

let baseOptions: Options<int> =
    { PopulationSize = 100
      SelectionRate = 0.8
      ...
      MutationRate = 0.05
      ... }
```

`SelectionRate = 0.8` means 20% of the population is left over every generation - a real pool for `elitist` and `uniform` to draw survivors from. `survivalRate` is chosen so that `SelectionRate + MutationRate + survivalRate = 1.0` (`0.8 + 0.05 + 0.15`), which keeps `elitist` and `uniform`'s population size roughly stable across generations. `pure` has no such balance: discarding `leftover` outright shrinks its population every generation until it settles into a small, mostly-static equilibrium - which is exactly what this example is built to make visible.

## How It Works

1. Run the OneMax-style problem for 300 generations, once with each reinsertion strategy plugged into `Options.ReinsertionFn` - everything else (population size, selection, crossover, mutation) stays identical across the three runs.
2. Record the best fitness seen at every generation via `Options.Probe`, so the three runs can be compared at matching points in time afterward.
3. Print a table of best fitness at every 30th generation, side by side for all three strategies.
4. Print each strategy's final fitness as a percentage of the maximum possible.

## Running the Example

From the repository root:

```powershell
dotnet run --project examples/Reinsertion/fsharp
```

## Expected Output

```text
Maximum possible fitness: 500 (all 500 genes set to 1)

Best fitness by generation (sampled every 30 generations):
Generation |     pure |  elitist |  uniform
         0 |    276.0 |    294.0 |    287.0
        30 |    309.0 |    376.0 |    358.0
        60 |    320.0 |    447.0 |    400.0
        90 |    326.0 |    490.0 |    456.0
       120 |    328.0 |    500.0 |    484.0
       150 |    327.0 |    500.0 |    500.0
       ...
       300 |    323.0 |    500.0 |    500.0

Final results:
pure     fitness: 323.0 / 500 ( 64.6% of maximum)
elitist  fitness: 500.0 / 500 (100.0% of maximum)
uniform  fitness: 500.0 / 500 (100.0% of maximum)
```

Across repeated runs, the same pattern shows up reliably: `elitist` and `uniform` both climb to the perfect score and hold it, while `pure` plateaus early - typically somewhere in the 60-70% range - and never gets close. Once `pure`'s shrinking population loses diversity, there's nothing left to recover it. Because the algorithm is randomized, the exact fitness values and generation counts will vary between runs, but the *shape* of the comparison - `pure` stalling well short of the maximum while `elitist`/`uniform` reach it - is consistent.

## Why This Example?

The other examples in this library each demonstrate a single configuration end to end. This one instead holds every setting fixed except one - the reinsertion strategy - specifically to make that one setting's effect visible. It's a useful companion to the `Reinsertion` module's own doc comments: where those describe what each strategy does, this example shows what that difference actually looks like over a real run.

## Related Projects

This example is part of the GeneticAlgorithms library and demonstrates its reinsertion strategies using the same style of binary, sum-based fitness function as the [`OneMaxProblem`](../../OneMaxProblem/fsharp/README.md) example.
