# Multiple Objectives Example

This project demonstrates a genetic-algorithm fitness function with two competing gene attributes: return on investment (ROI) and risk.

Each chromosome contains ten `(roi, risk)` pairs. Both values are randomly generated from `1` through `10`.

## Fitness Function

The current example combines both objectives into a single weighted fitness score:

```text
fitness = sum(2 * ROI - risk)
```

This rewards higher ROI and penalizes higher risk, with ROI given twice the weight of risk. The algorithm stops when a chromosome reaches a fitness of `180` or greater.

Although the example has two inputs to optimize, it currently uses a weighted-sum approach rather than Pareto multi-objective optimization. A future Pareto-based version would keep ROI and risk as independent fitness objectives and select non-dominated solutions.

## How It Works

1. Create a population of 125 chromosomes, each with ten random `(ROI, risk)` genes.
2. Evaluate every chromosome using the weighted fitness function.
3. Sort chromosomes by descending fitness.
4. Select parents, apply crossover and mutation, and repeat.
5. Stop when the target fitness is reached.

## Running the Example

From the repository root:

```powershell
dotnet run --project examples/MultipleObjectives/fsharp
```

The exact output varies because the initial population and genetic operations are random.
