# Hello World

This project demonstrates how to use the `GeneticAlgorithms` library to evolve a random string toward a target phrase using a genetic algorithm.

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

The purpose of this project is to provide a minimal and easy-to-understand example of a genetic algorithm working with character-based chromosomes instead of binary values. It demonstrates the core concepts of evolutionary computation:

* Population initialization
* Fitness evaluation
* Parent selection
* Crossover
* Mutation
* Evolution through generations

Because the problem is easy to visualize, it is a useful example for understanding how a population gradually approaches a target string.

## How It Works

1. Generate an initial population of random lowercase character strings.
2. Evaluate the fitness of each chromosome by comparing it with the target string.
3. Select parents from the population.
4. Create offspring through crossover.
5. Apply mutation to introduce variation.
6. Form the next generation.
7. Repeat until a chromosome reaches the termination threshold.

## Running the Example

From the repository root:

```powershell
dotnet run --project examples/HelloWorld
```

## Expected Output

The output will typically show the best fitness improving over time:

```text
Current Best 0.712500
Current Best 0.783333
Current Best 0.866667
...
Best solution: helloworld (fitness: 1.000000)
```

Because the example works with randomized populations, the intermediate fitness values will vary between runs.

## Why This Example?

This example is useful because it:

* Shows how to work with `char` chromosomes
* Demonstrates a string-based fitness function
* Produces output that is easy to interpret
* Provides a bridge between simple benchmark problems and more realistic search problems

Once this example is understood, the same approach can be extended to more advanced problems involving text, symbols, or structured candidate solutions.

## Related Projects

This example is part of the GeneticAlgorithms library and serves as a reference implementation for building and testing character-based genetic algorithm components.

