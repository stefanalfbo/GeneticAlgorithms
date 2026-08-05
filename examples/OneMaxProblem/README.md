# OneMax Problem

This project demonstrates how to use the `GeneticAlgorithms` library to solve the classic **OneMax Problem**, one of the most common benchmark problems in evolutionary computation.

## What is the OneMax Problem?

The goal of the OneMax problem is simple:

Given a binary chromosome consisting of `0`s and `1`s, maximize the number of `1`s.

For example:

```text
Chromosome: 1011010110
Fitness:    6
```

The optimal solution for a chromosome of length 10 is:

```text
1111111111
```

with a fitness value of:

```text
10
```

## Purpose

The purpose of this project is to provide a minimal and easy-to-understand example of a genetic algorithm in action. It demonstrates the core concepts of evolutionary computation:

* Population initialization
* Fitness evaluation
* Parent selection
* Crossover
* Mutation
* Evolution through generations

Because the fitness function is straightforward, the OneMax problem is an excellent starting point for validating and understanding the behavior of a genetic algorithm implementation.

## How It Works

1. Generate an initial population of random binary chromosomes.
2. Evaluate the fitness of each chromosome by counting the number of 1s.
3. Select parents from the population.
4. Create offspring through crossover.
5. Apply mutation to introduce variation.
6. Form the next generation.
7. Repeat until an optimal solution is found or the termination function decides to stop.

In this example, the termination function receives the current population, the generation number, and the temperature value, but it only uses the population because the fitness target is enough.

## Running the Example

From the repository root:

```powershell
dotnet run --project examples/OneMaxProblem
```

## Expected Output

The output will typically show the progress of the algorithm across generations:

```bash
Current Best 62
Current Best 67
Current Best 71
...
Current Best 999
Current Best 998
Current Best 1000
Best solution: 1000
```

Eventually the population should converge toward the optimal chromosome containing only 1s.

## Why OneMax?

The OneMax problem is widely used because it:

* Is easy to understand
* Has a known optimal solution
* Allows verification of genetic operators
* Serves as a baseline for more complex optimization problems

Once the algorithm works reliably on OneMax, it can be extended to more challenging domains such as:

* Traveling Salesman Problem (TSP)
* Scheduling
* Resource allocation
* Vehicle routing
* Constraint optimization problems

## Related Projects

This example is part of the GeneticAlgorithms library and serves as a reference implementation for building and testing genetic algorithm components.
