# Examples

This folder contains runnable example projects that demonstrate how to use the `GeneticAlgorithms` library for different kinds of optimization problems.

The examples are organized first by problem, then by language:

```text
examples/
  HelloWorld/
    csharp/
    fsharp/
  Knapsack/
    csharp/
    fsharp/
  MultipleObjectives/
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

A placeholder for multi-objective optimization examples. At the moment, this folder contains only the `fsharp` subfolder and does not yet include a runnable example project.

## Notes

* The F# examples use the native library API directly.
* The C# examples use the `GeneticAlgorithms.GeneticAlgorithm` facade.
* Example output varies between runs because the algorithms are randomized.
