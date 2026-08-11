# Hello World in C#

This project demonstrates how to use the `GeneticAlgorithms` library from C# to evolve a random string toward a target phrase using a genetic algorithm.

## What is the Hello World Problem?

The goal of this example is to evolve a chromosome of characters until it matches a target string.

In this project, the target is:

```text
helloworld
```

Each chromosome is a fixed-length array of lowercase characters from `a` to `z`.

The fitness function compares a candidate string with the target using Jaro similarity, where:

```text
1.0 = exact match
0.0 = no similarity
```

## Purpose

This example mirrors the F# HelloWorld version, but uses the C# facade provided by `GeneticAlgorithms.GeneticAlgorithm`. It shows how to define character-based genotype, fitness, and termination logic from C# while keeping the code close to the underlying genetic algorithm concepts.

## How It Works

1. Generate an initial population of random lowercase character strings.
2. Convert each chromosome to a string.
3. Compare each candidate with the target using Jaro similarity.
4. Select parents from the population.
5. Create offspring through crossover.
6. Apply mutation.
7. Continue until a chromosome exceeds the fitness threshold.

In this example, the termination callback receives the current population, generation, and temperature, but only the population is used because the fitness threshold is sufficient.

## Running the Example

From the repository root:

```powershell
dotnet run --project examples/HelloWorld/csharp
```

## Expected Output

The output will typically show improving similarity over time and end with a result like:

```text
Current Best 0.712500
Current Best 0.783333
Current Best 0.866667
...
Best solution: helloworld (fitness: 1.000000)
```

Because the algorithm is randomized, the intermediate values and the number of generations will vary between runs.
