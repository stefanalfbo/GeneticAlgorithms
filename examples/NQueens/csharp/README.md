# N-Queens Problem in C#

This project demonstrates how to use the `GeneticAlgorithms` library from C# to solve the classic **N-Queens Problem** with a genetic algorithm.

## What is the N-Queens Problem?

The N-Queens problem asks: how can `N` queens be placed on an `N`×`N` chessboard so that no two queens attack each other?

Two queens attack each other if they share:

* A row
* A column
* A diagonal

In this example, each chromosome is an array of length `N` where the index is the queen's column and the value is its row:

```text
genes[column] = row
```

Because the column is fixed by the gene's position, only row and diagonal conflicts need to be checked. The example solves the classic 8-queens board (`N = 8`).

## Purpose

This example mirrors the F# NQueens version, but uses the C# facade provided by `GeneticAlgorithms.GeneticAlgorithm`. It also shows how to pick a non-default selection or crossover strategy from C#: `GeneticAlgorithm.CreateOptions` has an overload that takes `Func`-typed selection and crossover delegates, so the library's own `Selection.elite` and `Crossover.orderOneCrossover` can be passed in directly as method groups.

## How It Works

1. Generate an initial population of chromosomes, each starting as `0, 1, ..., N - 1` shuffled into a random order, so every board starts with each row used exactly once.
2. Count the number of distinct rows used across the chromosome; row clashes reduce this count below `N`.
3. For every ordered pair of columns `(i, j)`, check whether the two queens sit on the same diagonal, i.e. whether `Math.Abs(i - j) == Math.Abs(genes[i] - genes[j])`.
4. Subtract the number of diagonal clashes found from the distinct-row count to get the fitness, so the maximum fitness (`N`) means every row is used exactly once and no diagonal clashes remain.
5. Continue until a chromosome reaches the maximum fitness.

Crossover uses `Crossover.orderOneCrossover` rather than the library's default single-point crossover. Because each chromosome is a permutation (every row used exactly once), a single-point cut would generally produce children with duplicate and missing rows; order-one crossover preserves the permutation instead, keeping the genotype's invariant intact across generations.

In this example, the termination callback receives the current population, generation, and temperature, but only the population is used because reaching the maximum fitness is sufficient.

## Running the Example

From the repository root:

```powershell
dotnet run --project examples/NQueens/csharp
```

## Expected Output

The output will typically show the best fitness improving over time and end with a conflict-free board:

```text
Current Best 4.000000
Current Best 8.000000
Best solution: [5; 7; 1; 3; 0; 6; 4; 2] (fitness: 8.000000 / 8.000000)
```

Because the algorithm is randomized, the intermediate values and the final board layout will vary between runs.
