# Rastrigin

This project demonstrates how to use the `GeneticAlgorithms` library to minimize the **Rastrigin function**, a classic benchmark for continuous (real-valued) optimization.

## What Is the Rastrigin Function?

In `n` dimensions:

```
f(x) = 10n + Σ (x_i² - 10·cos(2π·x_i))   for i = 1..n
```

It has a single global minimum of `f(x) = 0` at `x = (0, ..., 0)`, surrounded by a lattice of many shallower local minima (at every point with integer coordinates, `f` dips to a local low that isn't the global one). That lattice makes Rastrigin a standard stress test for optimizers: plain hill-climbing or gradient descent tends to get trapped in one of the local minima, while a genetic algorithm's population-wide exploration gives it a real chance to escape.

This example uses `n = 5` dimensions, each gene bounded to the standard `[-5.12, 5.12]` domain.

## The Genotype

Every other example in this library uses a discrete genotype - `int`, `char`, or a permutation. Rastrigin's genes are real-valued coordinates (`Chromosome<float>`), the first example to exercise the library's continuous-optimization support end to end.

## Fitness

`Genetic.run` always maximizes fitness, but Rastrigin is a function to *minimize*. Fitness is simply the negated value: the best chromosome has the fitness closest to zero, the largest (least negative) value.

## Why `wholeArithmeticCrossover` and `gaussian`?

Genes here are continuous coordinates, not discrete values to swap wholesale, so this example uses two library strategies built specifically for real-valued genotypes instead of the defaults:

- `Crossover.wholeArithmeticCrossover` blends each gene as a weighted average of both parents, rather than copying a value from one parent or the other outright - the only sensible way to combine continuous coordinates.
- `Mutation.gaussian` resamples each gene from a normal distribution fitted to the chromosome's own genes, rather than reordering existing values (`Mutation.scramble` would be meaningless here - there's no fixed multiset of gene values to reorder once genes are continuous coordinates). Exploration naturally narrows as the population converges toward the optimum.

## Why a Higher `MutationRate`, and Why `Reinsertion.elitist 0.05` Instead of `0.15`?

`SelectionRate = 0.8` combined with `Reinsertion.elitist` keeps the population size roughly stable across all 300 generations (`SelectionRate + MutationRate + survivalRate = 1.0`). Most other examples in this library use `MutationRate = 0.05` and `Reinsertion.elitist 0.15`, but Rastrigin's lattice of local minima needs more frequent mutants to reliably escape - this example raises `MutationRate` to `0.15` and lowers the elitist `survivalRate` to `0.05` to match (`0.8 + 0.15 + 0.05 = 1.0`). Raising `MutationRate` without lowering `survivalRate` to compensate would reintroduce the same unbounded population growth bug found and fixed elsewhere in this library's classic discrete examples.

This runs a fixed generation count with no early-exit fitness target, since continuous fitness rarely lands on an exact value to check against.

## Running the Example

From the repository root:

```powershell
dotnet run --project examples/Rastrigin/fsharp
```

## Expected Output

```text
Current best f(x): 28.7267
Current best f(x): 0.0000
Current best f(x): 0.0000
...

Best solution found (f(x) = 0.0000):
x = [0.0000, 0.0000, 0.0000, 0.0000, 0.0000]
```

Because the algorithm is randomized, the exact path to convergence varies between runs, but with these settings it reliably finds the global minimum well within the 300-generation cap.

## Related Projects

This example is part of the GeneticAlgorithms library and serves as the reference implementation for real-valued (continuous) optimization - complementing `TravelingSalesman`, which is also a real-valued minimization problem but over a permutation genotype rather than continuous coordinates.
