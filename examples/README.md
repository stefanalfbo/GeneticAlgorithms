# Examples

This folder contains runnable example projects that demonstrate how to use the `GeneticAlgorithms` library for different kinds of optimization problems.

The examples are organized first by problem, then by language:

```text
examples/
  CodeBreaker/
    csharp/
    fsharp/
  HelloWorld/
    csharp/
    fsharp/
  Knapsack/
    csharp/
    fsharp/
  MultipleObjectives/
    csharp/
    fsharp/
  NQueens/
    csharp/
    fsharp/
  OneMaxProblem/
    csharp/
    fsharp/
```

## Available Examples

### HelloWorld

A character-based optimization example that evolves random lowercase strings toward the target `helloworld` using Jaro similarity.

Run the F# version:

```powershell
dotnet run --project examples/HelloWorld/fsharp
```

Run the C# version:

```powershell
dotnet run --project examples/HelloWorld/csharp
```

### OneMaxProblem

A classic benchmark problem where the goal is to maximize the number of `1`s in a binary chromosome.

Run the F# version:

```powershell
dotnet run --project examples/OneMaxProblem/fsharp
```

Run the C# version:

```powershell
dotnet run --project examples/OneMaxProblem/csharp
```

### Knapsack

A constrained optimization example where binary genes determine which items are packed, and overweight solutions receive zero fitness.

Run the F# version:

```powershell
dotnet run --project examples/Knapsack/fsharp
```

Run the C# version:

```powershell
dotnet run --project examples/Knapsack/csharp
```

### MultipleObjectives

A weighted ROI/risk optimization example. The current implementation combines both values into one fitness score; it is not yet Pareto optimization.

Run the F# version:

```powershell
dotnet run --project examples/MultipleObjectives/fsharp
```

Run the C# version:

```powershell
dotnet run --project examples/MultipleObjectives/csharp
```

### NQueens

A constraint satisfaction example where the goal is to place `N` queens on an `N`×`N` board so that none attack each other. Fitness is the number of distinct rows used minus the number of diagonal clashes, so the maximum fitness means a fully valid board. Chromosomes are permutation-encoded, so this example uses `Crossover.orderOneCrossover` instead of the library's default single-point crossover to keep offspring valid permutations.

Run the F# version:

```powershell
dotnet run --project examples/NQueens/fsharp
```

Run the C# version:

```powershell
dotnet run --project examples/NQueens/csharp
```

### CodeBreaker

An XOR code-breaking example that evolves a binary candidate key until the encrypted message decodes to the known target. Fitness uses the Jaro distance to measure the similarity between the decoded and target strings.

Run the F# version:

```powershell
dotnet run --project examples/CodeBreaker/fsharp
```

Run the C# version:

```powershell
dotnet run --project examples/CodeBreaker/csharp
```

## Notes

* The F# examples use the native library API directly.
* The C# examples use the `GeneticAlgorithms.GeneticAlgorithm` facade.
* Example output varies between runs because the algorithms are randomized.
