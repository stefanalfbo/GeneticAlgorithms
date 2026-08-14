# Multiple Objectives Example in C#

This project is the C# counterpart to the F# Multiple Objectives example. It uses the `GeneticAlgorithms.GeneticAlgorithm` facade to evolve chromosomes containing ten `(ROI, risk)` gene pairs.

## Fitness Function

The example currently combines the two objectives into one weighted score:

```text
fitness = sum(2 * ROI - risk)
```

Higher ROI increases fitness and higher risk decreases it. The algorithm stops when a chromosome reaches a fitness of `180` or greater.

This is weighted single-objective optimization, not Pareto multi-objective optimization. A Pareto implementation would preserve ROI and risk as separate objectives.

## Running the Example

From the repository root:

```powershell
dotnet run --project examples/MultipleObjectives/csharp
```

Results vary because the population and genetic operations are randomized.
