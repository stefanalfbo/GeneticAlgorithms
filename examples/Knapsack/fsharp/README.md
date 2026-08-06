# Knapsack Problem

This project demonstrates how to use the `GeneticAlgorithms` library to solve a small **0/1 Knapsack Problem** with a genetic algorithm.

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

The purpose of this project is to provide a compact example of using a genetic algorithm for constrained optimization. Unlike OneMax, where every valid chromosome can be scored directly, the knapsack problem adds a hard capacity limit that invalidates some candidate solutions.

This makes the example useful for demonstrating:

* Population initialization
* Fitness evaluation with constraints
* Parent selection
* Crossover
* Mutation
* Evolution through generations

## How It Works

1. Generate an initial population of random binary chromosomes.
2. Interpret each gene as whether the corresponding item is packed.
3. Compute the total profit for the selected items.
4. Compute the total weight for the selected items.
5. Assign fitness `0` if the chromosome exceeds the weight limit.
6. Otherwise assign fitness equal to the total profit.
7. Repeat for a fixed number of generations.

In this example, the termination function receives the current population, the generation number, and the temperature value. It ignores the population and temperature and stops when the generation count reaches `1000`.

## Running the Example

From the repository root:

```powershell
dotnet run --project examples/Knapsack/fsharp
```

## Expected Output

The output will typically show the best fitness seen in each generation and end with the final chromosome:

```text
Current Best 33.000000
Current Best 35.000000
Current Best 41.000000
...
Best solution: [|0; 1; 1; 1; 0; 1; 0; 0; 0; 1|] (fitness: 35.000000)
```

Because the algorithm is randomized, the intermediate values and final chromosome may vary between runs.

## Why Knapsack?

The knapsack problem is widely used because it:

* Is easy to describe and reason about
* Has a clear tradeoff between value and constraint
* Demonstrates how infeasible solutions can be penalized
* Serves as a useful step beyond unconstrained benchmark problems

Once this example is understood, the same ideas can be extended to more advanced constrained optimization problems such as:

* Budget allocation
* Resource planning
* Scheduling with capacity limits
* Portfolio selection
* Packing and loading problems

## Related Projects

This example is part of the GeneticAlgorithms library and serves as a reference implementation for solving constrained binary optimization problems with a genetic algorithm.
