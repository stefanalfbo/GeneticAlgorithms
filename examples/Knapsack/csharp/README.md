# Knapsack Problem in C#

This project demonstrates how to use the `GeneticAlgorithms` library from C# to solve a small **0/1 Knapsack Problem** with a genetic algorithm.

## What is the Knapsack Problem?

The knapsack problem is a classic optimization problem:

Given a set of items with individual profits and weights, choose which items to include so that:

* The total weight stays within a fixed limit
* The total profit is as large as possible

In this example, each chromosome is a binary array where:

```text
1 = include the item
0 = exclude the item
```

The example uses these item values:

```text
Profits: [6, 5, 8, 9, 6, 7, 3, 1, 2, 6]
Weights: [10, 6, 8, 7, 10, 9, 7, 11, 6, 8]
Weight limit: 40
```

## Purpose

This example mirrors the F# Knapsack version, but uses the C# facade provided by `GeneticAlgorithms.GeneticAlgorithm`. It shows how to express a constrained optimization problem from C# while keeping the genetic algorithm setup compact.

## How It Works

1. Generate an initial population of random binary chromosomes.
2. Interpret each gene as whether the corresponding item is packed.
3. Compute the total profit for the selected items.
4. Compute the total weight for the selected items.
5. Assign fitness `0` if the chromosome exceeds the weight limit.
6. Otherwise assign fitness equal to the total profit.
7. Continue for a fixed number of generations.

In this example, the termination callback receives the current population, generation, and temperature, but only the generation is used because the example runs with a fixed generation cap.

## Running the Example

From the repository root:

```powershell
dotnet run --project examples/Knapsack/csharp
```

## Expected Output

The output will typically show the best fitness seen in each generation and end with a chromosome such as:

```text
Current Best 33.000000
Current Best 35.000000
Current Best 41.000000
...
Best solution: [0; 1; 1; 1; 0; 1; 0; 0; 0; 1] (fitness: 35.000000)
```

Because the algorithm is randomized, the intermediate values and final chromosome may vary between runs.
