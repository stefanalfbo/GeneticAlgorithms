# Rastrigin in C#

This project demonstrates how to use the `GeneticAlgorithms` library from C# to minimize the **Rastrigin function**, a classic benchmark for continuous (real-valued) optimization.

This example mirrors the F# `Rastrigin` version, but uses the C# facade provided by `GeneticAlgorithms.GeneticAlgorithm`.

## The Problem

In `n` dimensions, `f(x) = 10n + Σ (x_i² - 10·cos(2π·x_i))` has a single global minimum of `0` at `x = (0, ..., 0)`, surrounded by a lattice of shallower local minima that tend to trap simple optimizers. This example uses 5 dimensions, each gene bounded to `[-5.12, 5.12]`. `Genetic.run` always maximizes fitness, so fitness is the negated Rastrigin value.

## Real-Valued Genotype

Every other C# example uses a discrete genotype (`int` or `char`). This one uses `Chromosome<double>`, exercising the library's continuous-optimization support: `Crossover.wholeArithmeticCrossover` (blends genes as a weighted average of both parents) and `Mutation.gaussian` (resamples genes from a distribution fitted to the chromosome's own genes) instead of the discrete-genotype defaults.

## A New `CreateOptions` Overload

`GeneticAlgorithm.CreateOptions` previously had no way to change `MutationRate` from C# - every overload hardcoded it to `0.05`. Rastrigin's lattice of local minima needs a higher mutation rate (`0.15`) to reliably escape, so this example uses a new overload that also accepts `mutationRate`. Raising it without correspondingly lowering the elitist `survivalRate` (from the usual `0.15` down to `0.05`, keeping `SelectionRate + MutationRate + survivalRate = 1.0`) would reintroduce unbounded population growth, the same bug found and fixed elsewhere in this library's classic examples.

There's also no C#-facing equivalent of the F# `Probes.everyNth` combinator - throttling the progress output to every 30th generation is just a plain conditional inside the probe lambda.

## Running the Example

From the repository root:

```powershell
dotnet run --project examples/Rastrigin/csharp
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
