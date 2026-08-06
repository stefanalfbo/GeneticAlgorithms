# Genetic Algorithms

[![CI](https://github.com/stefanalfbo/GeneticAlgorithms/actions/workflows/ci.yml/badge.svg)](https://github.com/stefanalfbo/GeneticAlgorithms/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

![Genetic Algorithms logo](assets/logo.svg)

`GeneticAlgorithms` is a small F# library for experimenting with genetic algorithms. It provides a compact, generic execution pipeline for evolving chromosomes and includes example projects that demonstrate both binary and character-based optimization problems.

The repository is structured as an educational, easy-to-read implementation rather than a feature-complete optimization framework. The core abstractions are intentionally small so you can understand how the algorithm works and adapt it for your own problems.

## Inspiration

This library is heavily influenced by the ideas and teaching approach in *Genetic Algorithms in Elixir: Solve Problems Using Evolution* by Sean Moriarity. If you want a practical introduction to evolutionary algorithms and how to structure them in code, it is a strong companion resource for this repository.

## Features

* Generic chromosome representation with support for any gene type
* Problem definition through pluggable genotype, fitness, and termination functions
* Population evaluation with age tracking and fitness sorting
* Parent pairing for even and odd population sizes
* Single-point crossover
* Mutation by random gene shuffling
* Included examples and automated tests

## Project Structure

```text
src/GeneticAlgorithms                Core library
examples/HelloWorld/fsharp           F# character-based string evolution example
examples/OneMaxProblem/fsharp        F# binary optimization benchmark example
examples/Knapsack/fsharp             F# constrained optimization example
tests/GeneticAlgorithms.Tests        Automated tests with Expecto
```

## Core Concepts

The library revolves around three types:

### `Chromosome<'T>`

Represents a candidate solution.

```fsharp
type Chromosome<'T> =
    { genes: 'T array
      size: int
      fitness: float
      age: int }
```

### `Problem<'Gene>`

Defines how a specific optimization problem behaves.

```fsharp
type Problem<'Gene> =
    { genotype: unit -> Chromosome<'Gene>
      fitness_function: Chromosome<'Gene> -> float
      terminate: seq<Chromosome<'Gene>> -> int -> float -> bool }
```

### `Options`

Controls runtime configuration.

```fsharp
type Options = { population_size: int }
```

## Algorithm Flow

`Genetic.run` executes the following loop:

1. Initialize a population using the supplied genotype function.
2. Evaluate all chromosomes with the fitness function.
3. Sort the population by descending fitness.
4. Select parents by pairing neighboring chromosomes.
5. Produce children using single-point crossover.
6. Apply mutation to some chromosomes.
7. Repeat until the termination function returns `true` for the current population, generation, and temperature.

During execution, the current best fitness is printed for each generation. The termination callback receives the evaluated population, the current generation number, and a temperature value computed from recent fitness progress, so problems can stop either on solution quality, a generation cap, temperature behavior, or a combination of those signals.

## Getting Started

### Requirements

* .NET 9 SDK

### Build the solution

From the repository root:

```powershell
dotnet build genetic-algorithms.sln
```

### Run the tests

```powershell
dotnet run --project tests/GeneticAlgorithms.Tests/GeneticAlgorithms.Tests.fsproj
```

## Basic Usage

Define a genotype function, a fitness function, and a termination condition, then call `Genetic.run`.

```fsharp
open GeneticAlgorithms

let genotype () =
    let genes = Array.init 10 (fun _ -> System.Random.Shared.Next(0, 2))

    { genes = genes
      size = genes.Length
      fitness = 0.0
      age = 0 }

let fitness_function (chromosome: Chromosome<int>) =
    chromosome.genes |> Array.sum |> float

let terminate (population: seq<Chromosome<int>>) (_generation: int) (_temperature: float) =
    population |> Seq.exists (fun chromosome -> chromosome.fitness >= 10.0)

let problem: Problem<int> =
    { genotype = genotype
      fitness_function = fitness_function
      terminate = terminate }

let options = { population_size = 100 }

let solution = Genetic.run problem options
```

## Examples

### HelloWorld

The HelloWorld example evolves a random lowercase character string toward the target `helloworld`. Its fitness function uses Jaro similarity, which makes it a simple example of working with `char` chromosomes instead of binary genes.

Its termination function checks the current population and ignores the generation and temperature arguments because the fitness threshold alone is enough for this example.

Run it with:

```powershell
dotnet run --project examples/HelloWorld/fsharp
```

Typical output ends with a string result similar to:

```text
Best solution: helloworld (fitness: 1.000000)
```

### OneMaxProblem

The OneMax example solves the classic benchmark problem of maximizing the number of `1`s in a binary chromosome.

Like HelloWorld, it stops based on population fitness and ignores the generation and temperature arguments passed to the termination callback.

Run it with:

```powershell
dotnet run --project examples/OneMaxProblem/fsharp
```

### Knapsack

The Knapsack example solves a small 0/1 knapsack problem where binary genes indicate whether an item is packed. Candidate solutions that exceed the weight limit receive zero fitness.

Run it with:

```powershell
dotnet run --project examples/Knapsack/fsharp
```

This example is useful for exploring constrained optimization rather than pure maximization.

This is a useful baseline for validating the library's evaluation, selection, crossover, and mutation behavior.

## Test Coverage

The test project currently verifies the main building blocks of the algorithm:

* `Genetic.evaluate` applies fitness, increments age, and sorts by descending fitness
* `Genetic.select` pairs chromosomes correctly for even and odd populations
* `Genetic.crossover` preserves chromosome size and recombines parent genes
* `Genetic.mutation` preserves population size and gene membership
* `Genetic.initialize` creates the requested number of chromosomes
* `Genetic.run` returns the fittest chromosome when termination is reached
* `Genetic.run` forwards generation and temperature values to the termination callback

## Design Notes

This implementation is intentionally minimal. A few design choices to be aware of:

* Mutation currently shuffles the genes within a chromosome rather than replacing individual genes with newly generated values
* Selection pairs adjacent chromosomes after sorting rather than using tournament or roulette-wheel selection
* The runtime options currently expose only population size

Those constraints keep the code simple, but they also make the project a good starting point for extending the algorithm with stronger selection strategies, richer mutation operators, elitism, configurable stopping criteria, or additional runtime parameters.

## Repository Goals

This project is a good fit if you want to:

* Learn how a genetic algorithm can be implemented in F#
* Experiment with generic chromosome representations
* Build on a small codebase instead of adopting a large framework
* Add new example problems and compare evolutionary behavior

## License

This repository is licensed under the terms of the LICENSE file in the project root.
