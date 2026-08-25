# Schedule

This project demonstrates how to use the `GeneticAlgorithms` library to solve a **class scheduling** problem: choosing which classes to take, under a credit-hour limit, to maximize interest and usefulness while minimizing difficulty.

This is the class scheduling example from *Genetic Algorithms in Elixir* (Chapter 8, "Replacing and Transitioning").

## What is the Class Scheduling Problem?

Given ten possible classes - Algorithms, Artificial Intelligence, Calculus, Chemistry, Data Structures, Discrete Math, History, Literature, Physics, and Volleyball - decide which ones to take, subject to a limit of 18 credit hours, so that the overall schedule scores as highly as possible on interest and usefulness while scoring as low as possible on difficulty.

Each class has been rated from 1 to 10 on three criteria, plus a fixed credit-hour cost:

| Class | Interest | Usefulness | Difficulty | Credit Hours |
| --- | --- | --- | --- | --- |
| Algorithms | 8.0 | 8.0 | 8.0 | 3.0 |
| Artificial Intelligence | 8.0 | 9.0 | 9.0 | 3.0 |
| Calculus | 5.0 | 6.0 | 4.0 | 3.0 |
| Chemistry | 9.0 | 2.0 | 3.0 | 4.5 |
| Data Structures | 7.0 | 8.0 | 5.0 | 3.0 |
| Discrete Math | 2.0 | 9.0 | 2.0 | 3.0 |
| History | 8.0 | 1.0 | 4.0 | 3.0 |
| Literature | 2.0 | 2.0 | 2.0 | 3.0 |
| Physics | 7.0 | 5.0 | 6.0 | 4.5 |
| Volleyball | 10.0 | 1.0 | 1.0 | 1.5 |

In this example, each chromosome is a fixed-length binary array where the index is a class and the value is whether it's included in the schedule:

```text
gene[class] = 1 (taking it) or 0 (not taking it)
```

## Purpose

The purpose of this project is to provide an example of **multi-objective, constrained optimization**: three competing criteria (interest, usefulness, difficulty) are combined into a single weighted score, and a hard constraint (the 18-credit-hour limit) is enforced through the fitness function itself rather than by restricting what a chromosome can represent. This makes the example useful for demonstrating:

* Population initialization
* Fitness evaluation with weighted, competing objectives
* Constraint handling via a fitness penalty
* Parent selection
* Crossover
* Mutation
* Evolution through generations

## How It Works

1. Generate an initial population of random binary chromosomes, one gene per class.
2. For every selected class, add `0.3 * usefulness + 0.3 * interest - 0.3 * difficulty` to the schedule's fitness - so usefulness and interest raise the score, and difficulty lowers it, all weighted equally.
3. Add up the credit hours of every selected class.
4. If the total exceeds 18 credit hours, the schedule's fitness becomes `-99999` regardless of how good its weighted score was - a large enough penalty that invalid schedules always lose to valid ones.
5. Select parents, apply crossover and mutation, and repeat.
6. Stop once the algorithm has run for 1000 generations.

Because the optimal fitness value isn't known ahead of time (unlike, say, `NQueens`, where a fully valid board has a known maximum score), this example terminates on a fixed generation count instead of a fitness target.

## Running the Example

From the repository root:

```powershell
dotnet run --project examples/Schedule/fsharp
```

## Expected Output

The output will typically show the best fitness improving over time and end with the winning schedule, both as raw genes and as class names:

```text
Current Best 9.900000
Current Best 12.300000
Current Best 14.700000
Best schedule: [|1; 0; 0; 0; 1; 1; 1; 0; 0; 1|] (fitness: 12.600000)
Classes:       Algorithms, Data Structures, Discrete Math, History, Volleyball
```

Because the algorithm is randomized, the intermediate values and the final schedule will vary between runs.

## Why Schedule?

The class scheduling problem is a useful step beyond single-objective benchmarks because it:

* Combines several competing objectives into one fitness score with fixed weights
* Enforces a hard constraint through a large fitness penalty rather than by restricting the genotype itself
* Is easy to relate to and reason about by hand, since the "correct" tradeoffs are intuitive
* Serves as a bridge toward more realistic resource-allocation and preference-optimization problems

## Related Projects

This example is part of the GeneticAlgorithms library and serves as a reference implementation for solving multi-objective, constrained optimization problems with a genetic algorithm.
