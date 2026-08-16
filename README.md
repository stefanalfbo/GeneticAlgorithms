# Genetic Algorithms

[![CI](https://github.com/stefanalfbo/GeneticAlgorithms/actions/workflows/ci.yml/badge.svg)](https://github.com/stefanalfbo/GeneticAlgorithms/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/GeneticAlgorithms.svg)](https://www.nuget.org/packages/GeneticAlgorithms)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

![Genetic Algorithms logo](https://raw.githubusercontent.com/stefanalfbo/GeneticAlgorithms/main/assets/logo.svg)

`GeneticAlgorithms` is a small F# library for experimenting with genetic algorithms. It provides a compact, generic execution pipeline for evolving chromosomes and includes example projects that demonstrate both binary and character-based optimization problems.

The repository is structured as an educational, easy-to-read implementation rather than a feature-complete optimization framework. The core abstractions are intentionally small so you can understand how the algorithm works and adapt it for your own problems.

## Inspiration

This library is heavily influenced by the ideas and teaching approach in [Genetic Algorithms in Elixir: Solve Problems Using Evolution](https://pragprog.com/titles/smgaelixir/genetic-algorithms-in-elixir/) by Sean Moriarity. If you want a practical introduction to evolutionary algorithms and how to structure them in code, it is a strong companion resource for this repository.

## Installation

The library is published on [nuget.org](https://www.nuget.org/packages/GeneticAlgorithms):

```powershell
dotnet add package GeneticAlgorithms
```

## Features

* Generic chromosome representation with support for any gene type
* Problem definition through pluggable genotype, fitness, and termination functions
* Population evaluation with age tracking and fitness sorting
* Configurable parent selection: elite, random, tournament, tournament without duplicates, or roulette-wheel
* Single-point crossover
* Configurable mutation rate, applied by random gene shuffling
* Included examples and automated tests

## Project Structure

```text
src/GeneticAlgorithms                Core library
examples/                            Example problems, each with F# and/or C# variants - see examples/README.md
tests/GeneticAlgorithms.Tests         Automated tests with Expecto
tests/GeneticAlgorithms.CSharpSmoke   Minimal C# consumer validating the interop layer
tests/GeneticAlgorithms.NuGetSmoke    Verifies a published NuGet release installs and runs correctly
```

## Core Concepts

The library revolves around three types:

### `Chromosome<'T>`

Represents a candidate solution. `Size` is derived from `Genes` rather than stored separately.

```fsharp
type Chromosome<'T> =
    { Genes: 'T array
      Fitness: float
      Age: int }

    member this.Size = this.Genes.Length
```

### `Problem<'Gene>`

Defines how a specific optimization problem behaves.

```fsharp
type Problem<'Gene> =
    { Genotype: unit -> Chromosome<'Gene>
      FitnessFunction: Chromosome<'Gene> -> float
      Terminate: seq<Chromosome<'Gene>> -> int -> float -> bool }
```

### `Options<'Gene>`

Controls runtime configuration.

```fsharp
type Options<'Gene> =
    { PopulationSize: int
      SelectionRate: float
      SelectionFn: Chromosome<'Gene> array -> int -> Chromosome<'Gene> array
      MutationRate: float
      OnGeneration: Chromosome<'Gene> -> int -> unit }
```

`SelectionFn` picks from the `Selection` module (`Selection.elite`, `Selection.random`, `Selection.tournament`, `Selection.tournamentNoDuplicates`, `Selection.roulette`) or a custom function of the same shape. `OnGeneration` is called with the current generation's best chromosome after every evaluation, so callers decide whether and how to report progress - `Genetic.printProgress` is a ready-made implementation that prints the best fitness.

## Algorithm Flow

`Genetic.run` executes the following loop:

1. Initialize a population using the supplied genotype function.
2. Evaluate all chromosomes with the fitness function and sort by descending fitness.
3. Report progress via `OnGeneration`.
4. Stop if the termination function returns `true` for the current population, generation, and temperature.
5. Otherwise, select parents using `SelectionFn` and `SelectionRate`, keeping any unselected chromosomes as leftover.
6. Produce children from the selected parents using single-point crossover.
7. Combine children with the leftover chromosomes and apply mutation at `MutationRate`.
8. Repeat from step 2 with the resulting population.

The termination callback receives the evaluated population, the current generation number, and a temperature value computed from recent fitness progress, so problems can stop either on solution quality, a generation cap, temperature behavior, or a combination of those signals.

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

    { Genes = genes
      Fitness = 0.0
      Age = 0 }

let fitness_function (chromosome: Chromosome<int>) =
    chromosome.Genes |> Array.sum |> float

let terminate (population: seq<Chromosome<int>>) (_generation: int) (_temperature: float) =
    population |> Seq.exists (fun chromosome -> chromosome.Fitness >= 10.0)

let problem: Problem<int> =
    { Genotype = genotype
      FitnessFunction = fitness_function
      Terminate = terminate }

let options =
    { PopulationSize = 100
      SelectionRate = 0.8
      SelectionFn = Selection.elite
      MutationRate = 0.05
      OnGeneration = Genetic.printProgress }

let solution = Genetic.run problem options
```

## C# Interop

The core API is implemented in idiomatic F#, but the library also exposes a small C#-friendly facade through `GeneticAlgorithms.GeneticAlgorithm`. This avoids forcing C# examples to construct F# records with curried function fields directly.

```csharp
using GeneticAlgorithms;

var solution = GeneticAlgorithm.Run(
  genotype: () => GeneticAlgorithm.CreateChromosome(new[] { Random.Shared.Next(0, 2) }),
  fitnessFunction: chromosome => chromosome.Genes[0],
  terminate: (population, generation, temperature) =>
    population.Any(chromosome => chromosome.Fitness >= 1.0) || generation >= 10,
  populationSize: 8);
```

The older `Interop` type remains available as a compatibility wrapper, but new C# examples should prefer `GeneticAlgorithm`.

The smoke project in `tests/GeneticAlgorithms.CSharpSmoke` exists specifically to validate that this API stays straightforward to consume from C#.

## Examples

See [examples/README.md](examples/README.md) for the full index of example projects, what each one demonstrates, and their available language variants.

## Test Coverage

The test project verifies the main building blocks of the algorithm:

* `Genetic.evaluate` applies fitness, increments age, and sorts by descending fitness
* `Genetic.crossover` preserves chromosome size and recombines parent genes
* `Genetic.mutation` preserves population size and gene membership
* `Genetic.initialize` creates the requested number of chromosomes
* `Genetic.run` returns the fittest chromosome when termination is reached
* `Genetic.run` forwards generation and temperature values to the termination callback
* `Selection.elite`, `Selection.random`, `Selection.tournament`, `Selection.tournamentNoDuplicates`, and `Selection.roulette` each return the requested number of chromosomes under their respective selection rules
* `Selection.select` splits a population into parent pairs and leftover chromosomes according to `SelectionRate`, rounding odd counts up to stay even

## Design Notes

This implementation is intentionally minimal. A few design choices to be aware of:

* Mutation shuffles the genes within a chromosome rather than replacing individual genes with newly generated values
* Crossover is single-point; there is no configurable crossover rate
* Randomness always comes from `System.Random.Shared`, so evolution runs are not seedable or reproducible

Those constraints keep the code simple, but they also make the project a good starting point for extending the algorithm with richer mutation operators, alternative crossover strategies, or seedable randomness for reproducible runs.

## Repository Goals

This project is a good fit if you want to:

* Learn how a genetic algorithm can be implemented in F#
* Experiment with generic chromosome representations
* Build on a small codebase instead of adopting a large framework
* Add new example problems and compare evolutionary behavior

## License

This repository is licensed under the terms of the LICENSE file in the project root.
