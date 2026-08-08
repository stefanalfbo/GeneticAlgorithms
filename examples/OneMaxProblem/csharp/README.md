# OneMax Problem in C#

This project demonstrates how to use the `GeneticAlgorithms` library from C# to solve the classic **OneMax Problem**.

## What is the OneMax Problem?

The goal of the OneMax problem is to maximize the number of `1`s in a binary chromosome.

For example:

```text
Chromosome: 1011010110
Fitness:    6
```

The optimal chromosome for a length of 10 is:

```text
1111111111
```

with fitness:

```text
10
```

## Purpose

This example mirrors the F# version of OneMax, but uses the C# facade provided by `GeneticAlgorithms.GeneticAlgorithm`. It shows how to define the genotype, fitness function, and termination condition from C# without dealing directly with F#-specific function shapes.

## How It Works

1. Generate an initial population of random binary chromosomes.
2. Evaluate each chromosome by summing its genes.
3. Select parents from the population.
4. Create offspring through crossover.
5. Apply mutation.
6. Continue until a chromosome reaches the target fitness.

In this example, the termination callback receives the current population, generation, and temperature, but only the population is used because the fitness target is sufficient.

## Running the Example

From the repository root:

```powershell
dotnet run --project examples/OneMaxProblem/csharp
```

## Expected Output

The output will typically show progress across generations and end with a result like:

```text
Current Best 61.000000
Current Best 73.000000
Current Best 88.000000
...
Current Best 1000.000000
Best solution: 1000
```

Because the algorithm is randomized, the intermediate values will vary between runs.
