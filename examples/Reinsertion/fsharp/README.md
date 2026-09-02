# Reinsertion

This project demonstrates the three **reinsertion strategies** in the `GeneticAlgorithms` library's `Reinsertion` module - `pure`, `elitist`, and `uniform` - by running the exact same problem three times, once per strategy, and comparing the results side by side.

It reuses the class-scheduling problem from the [`Schedule`](../../Schedule/fsharp/README.md) example unchanged. See that README for the full problem description; this one focuses on what reinsertion is and how the three strategies differ.

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

1. Run the class-scheduling problem for 1000 generations, once with each reinsertion strategy plugged into `Options.ReinsertionFn` - everything else (population size, selection, crossover, mutation) stays identical across the three runs.
2. Record the best fitness seen at every generation via `Options.OnGeneration`, so the three runs can be compared at matching points in time afterward.
3. Print a table of best fitness at every 100th generation, side by side for all three strategies.
4. Print each strategy's final schedule.

## Running the Example

From the repository root:

```powershell
dotnet run --project examples/Reinsertion/fsharp
```

## Expected Output

```text
Best fitness by generation (sampled every 100 generations):
Generation |     pure |  elitist |  uniform
         0 |    15.30 |    14.70 |    14.70
       100 |    13.20 |    15.90 |    15.90
       200 |    15.00 |    15.90 |    15.90
       ...
      1000 |    10.80 |    15.90 |    15.90

Final schedules:
pure     fitness:    10.80  classes: Artificial Intelligence, Calculus, Discrete Math, Literature, Volleyball
elitist  fitness:    15.90  classes: Algorithms, Artificial Intelligence, Chemistry, Data Structures, Discrete Math, Volleyball
uniform  fitness:    15.90  classes: Algorithms, Artificial Intelligence, Chemistry, Data Structures, Discrete Math, Volleyball
```

Across repeated runs, the same pattern shows up reliably: `elitist` and `uniform` both find the best schedule and hold onto it once found, while `pure` fluctuates and often regresses - once its shrinking population loses a good gene, there's nothing left to bring it back. Because the algorithm is randomized, the exact fitness values and generation counts will vary between runs, but the *shape* of the comparison - `pure` degrading while `elitist`/`uniform` stay stable - is consistent.

## Why This Example?

The other examples in this library each demonstrate a single configuration end to end. This one instead holds every setting fixed except one - the reinsertion strategy - specifically to make that one setting's effect visible. It's a useful companion to the `Reinsertion` module's own doc comments: where those describe what each strategy does, this example shows what that difference actually looks like over a real run.

## Related Projects

This example is part of the GeneticAlgorithms library and builds on the [`Schedule`](../../Schedule/fsharp/README.md) example's problem definition to demonstrate the library's reinsertion strategies.
