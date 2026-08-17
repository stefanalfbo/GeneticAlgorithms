# N-Queens Problem

This project demonstrates how to use the `GeneticAlgorithms` library to solve the classic **N-Queens Problem** with a genetic algorithm.

## What is the N-Queens Problem?

The N-Queens problem asks: how can `N` queens be placed on an `N`×`N` chessboard so that no two queens attack each other?

Two queens attack each other if they share:

* A row
* A column
* A diagonal

In this example, each chromosome is an array of length `N` where the index is the queen's column and the value is its row:

```text
gene[column] = row
```

Because the column is fixed by the gene's position, only row and diagonal conflicts need to be checked.

The example solves the classic 8-queens board (`N = 8`).

## Purpose

The purpose of this project is to provide an example of using a genetic algorithm to solve a constraint satisfaction problem, rather than a value-maximization problem like Knapsack. Fitness here measures how close a candidate is to a fully valid layout, not how "good" it is.

This makes the example useful for demonstrating:

* Population initialization
* Fitness evaluation based on constraint violations
* Parent selection
* Crossover
* Mutation
* Evolution through generations

## How It Works

1. Generate an initial population of chromosomes, each starting as `[0; 1; ...; N - 1]` shuffled into a random order, so every board starts with each row used exactly once.
2. Count the number of distinct rows used across the chromosome; row clashes reduce this count below `N`.
3. For every ordered pair of columns `(i, j)`, check whether the two queens sit on the same diagonal, i.e. whether `abs(i - j) = abs(gene[i] - gene[j])`.
4. Subtract the number of diagonal clashes found from the distinct-row count to get the fitness, so the maximum fitness (`N`) means every row is used exactly once and no diagonal clashes remain.
5. Repeat until a chromosome reaches the maximum fitness.

In this example, the termination function receives the current population, the generation number, and the temperature value, but it only uses the population because reaching the maximum fitness is sufficient.

Crossover uses `Crossover.orderOneCrossover` rather than the library's default single-point crossover. Because each chromosome is a permutation (every row used exactly once), a single-point cut would generally produce children with duplicate and missing rows; order-one crossover preserves the permutation instead, keeping the genotype's invariant intact across generations.

## Running the Example

From the repository root:

```powershell
dotnet run --project examples/NQueens/fsharp
```

## Expected Output

The output will typically show the best fitness improving over time and end with a conflict-free board:

```text
Current Best 4.000000
Current Best 5.000000
Current Best 6.000000
...
Best solution: [|2; 4; 7; 3; 0; 6; 1; 5|] (fitness: 8.000000 / 8.000000)
```

Because the algorithm is randomized, the intermediate values and the final board layout will vary between runs.

## Why N-Queens?

The N-Queens problem is widely used because it:

* Has a fitness landscape shaped entirely by constraint violations rather than a value to maximize
* Is easy to visualize as a board
* Scales in difficulty as `N` grows
* Serves as a bridge between simple benchmark problems and general constraint satisfaction problems

Once this example is understood, the same ideas can be extended to other constraint satisfaction problems such as:

* Scheduling without conflicts
* Graph coloring
* Sudoku-style puzzles
* Resource allocation with exclusivity constraints

## Related Projects

This example is part of the GeneticAlgorithms library and serves as a reference implementation for solving constraint satisfaction problems with a genetic algorithm.
